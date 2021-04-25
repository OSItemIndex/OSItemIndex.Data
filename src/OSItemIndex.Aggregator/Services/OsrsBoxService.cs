using System;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using OSItemIndex.Aggregator.Utils;
using OSItemIndex.Data.Database;
using OSItemIndex.Data.Extensions;
using Serilog;

namespace OSItemIndex.Aggregator.Services
{
    public class OsrsBoxService : AggregateService, IOsrsBoxService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IDbContextHelper _dbContextHelper;

        public OsrsBoxService(IHttpClientFactory httpFactory, IDbContextHelper dbContextHelper) : base("osrsbox", TimeSpan.FromSeconds(2))
        {
            _httpFactory = httpFactory;
            _dbContextHelper = dbContextHelper;
        }

        public async Task<ReleaseMonitoringProject?> GetReleaseMonitoringProjectAsync()
        {
            using var client = _httpFactory.CreateClient("osrsbox");
            using var request = new HttpRequestMessage(HttpMethod.Get, Endpoints.OsrsBox.Project);
            using var response = await client.SendAsync(request);
            using var stream = await response.Content.ReadAsStreamAsync();
            try
            {
                response.EnsureSuccessStatusCode();
                return await JsonSerializer.DeserializeAsync<ReleaseMonitoringProject>(stream);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to get pypi details {@Response}", response);
            }
            return null;
        }

        public async Task<HttpResponseMessage> GetFullItemsSummaryResponseAsync()
        {
            var client = _httpFactory.CreateClient("osrsbox");
            using var factory = _dbContextHelper.GetFactory();
            using var request = new HttpRequestMessage(HttpMethod.Get, Endpoints.OsrsBox.ItemsComplete);
            {
                var response = await client.SendAsync(request);
                try
                {
                    response.EnsureSuccessStatusCode();
                    return response;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to get items-summary.json {@Response}", response);
                }
                return response;
            }
        }

        public async Task AggregateAndBulkCopyItemsAsync()
        {
            using var response = await GetFullItemsSummaryResponseAsync();
            {
                if (!response.IsSuccessStatusCode)
                    return;
            }
            using var factory = _dbContextHelper.GetFactory();
            using var stream = await response.Content.ReadAsStreamAsync();
            {
                var dbContext = factory.GetDbContext();
                var conn = dbContext.Database.GetNpgsqlConnection();
                var connOpenedHere = await conn.EnsureConnectedAsync();
                var transaction = dbContext.EnsureOrStartTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    const string tempTable = "_temp_items";
                    var sql = "CREATE TEMP TABLE _temp_items (LIKE items INCLUDING DEFAULTS) ON COMMIT DROP";
                    await conn.ExecuteNonQueryAsync(sql); // Create temp table

                    // COPY into temp table
                    using (var importer = conn.BeginBinaryImport("COPY _temp_items (id, name, duplicate, noted, placeholder, stackable, tradeable_on_ge, last_updated, document) FROM STDIN (FORMAT BINARY)"))
                    {
                        ImportFromContentStream(stream, importer);
                        await importer.CompleteAsync();
                    }

                    // Lock table
                    sql = "LOCK TABLE prices_realtime IN ACCESS EXCLUSIVE MODE";
                    await conn.ExecuteNonQueryAsync(sql);

                    sql = @"INSERT INTO items SELECT * FROM _temp_items ON CONFLICT (id) DO UPDATE SET
                                id=EXCLUDED.id,
                                name=EXCLUDED.name,
                                duplicate=EXCLUDED.duplicate,
                                noted=EXCLUDED.noted,
                                placeholder=EXCLUDED.placeholder,
                                stackable=EXCLUDED.stackable,
                                tradeable_on_ge=EXCLUDED.tradeable_on_ge,
                                last_updated=EXCLUDED.last_updated,
                                document=EXCLUDED.document";

                    await conn.ExecuteNonQueryAsync(sql);
                    if (transaction != null)
                        await transaction.CommitAsync();
                }
                catch
                {
                    try
                    {
                        if (transaction != null)
                            await transaction.RollbackAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                    throw;
                }
                finally
                {
                    if (connOpenedHere)
                        await conn.CloseAsync();
                }
            }
        }

        public static void ImportFromContentStream(Stream stream, NpgsqlBinaryImporter importer)
        {
            using var jSr = new Utf8JsonStreamReader(stream, 32 * 1024);
            while (jSr.Read())
            {
                if (jSr.CurrentDepth != 1 || jSr.TokenType != JsonTokenType.StartObject)
                    continue;

                using var doc = jSr.GetJsonDocument();
                {
                    importer.StartRow();
                    importer.Write(doc.RootElement.GetProperty("id").GetInt32());
                    importer.Write(doc.RootElement.GetProperty("name").GetString());
                    importer.Write(doc.RootElement.GetProperty("duplicate").GetBoolean());
                    importer.Write(doc.RootElement.GetProperty("noted").GetBoolean());
                    importer.Write(doc.RootElement.GetProperty("placeholder").GetBoolean());
                    importer.Write(doc.RootElement.GetProperty("stackable").GetBoolean());
                    importer.Write(doc.RootElement.GetProperty("tradeable_on_ge").GetBoolean());
                    importer.Write(doc.RootElement.GetProperty("last_updated").GetDateTime());
                    importer.Write(doc.RootElement.GetRawText());
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await AggregateAndBulkCopyItemsAsync();
        }
    }
}
