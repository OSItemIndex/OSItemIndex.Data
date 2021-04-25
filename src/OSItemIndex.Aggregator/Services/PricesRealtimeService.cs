using System;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using OSItemIndex.Aggregator.Utils;
using OSItemIndex.Data;
using OSItemIndex.Data.Database;
using OSItemIndex.Data.Extensions;
using Serilog;

namespace OSItemIndex.Aggregator.Services
{
    public class PricesRealtimeService : AggregateService, IPricesRealtimeService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IDbContextHelper _dbContextHelper;
        private readonly string _pricesTable;

        public PricesRealtimeService(IHttpClientFactory httpFactory, IDbContextHelper dbContextHelper) : base("prices-realtime", TimeSpan.FromSeconds(2))
        {
            _httpFactory = httpFactory;
            _dbContextHelper = dbContextHelper;

            using var factory = _dbContextHelper.GetFactory();
            {
                var dbContext = factory.GetDbContext();
                var model = dbContext.Model;
                _pricesTable = model.FindEntityType(typeof(RealtimePrice)).GetTableName();
            }
        }

        public async Task<HttpResponseMessage> GetLatestPricesAsync() => await GetModelResponseAsync(Endpoints.Realtime.PriceLatest);
        public async Task<HttpResponseMessage> GetFiveMinutePricesAsync() => await GetModelResponseAsync(Endpoints.Realtime.PriceFiveMin);
        public async Task<HttpResponseMessage> GetOneHourPricesAsync() => await GetModelResponseAsync(Endpoints.Realtime.PriceOneHour);

        private async Task<HttpResponseMessage> GetModelResponseAsync(string uri)
        {
            var client = _httpFactory.CreateClient("realtimeprices");
            using var factory = _dbContextHelper.GetFactory();
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            {
                var response = await client.SendAsync(request);
                try
                {
                    response.EnsureSuccessStatusCode();
                    return response;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to get realtime prices model {@Response}", response);
                }
                return response;
            }
        }

        private static string[] ModelUris { get; } = { Endpoints.Realtime.PriceLatest, Endpoints.Realtime.PriceFiveMin, Endpoints.Realtime.PriceOneHour };
        private static string[] ModelColumns { get; } = { "latest", "five_minute", "one_hour" };
        private enum Model : int { Latest, FiveMinute, OneHour }

        public async Task AggregateAndBulkCopyLatestAsync() => await AggregateAndBulkCopyAsync(Model.Latest);
        public async Task AggregateAndBulkCopyFiveMinuteAsync() => await AggregateAndBulkCopyAsync(Model.FiveMinute);
        public async Task AggregateAndBulkCopyOneHourAsync() => await AggregateAndBulkCopyAsync(Model.OneHour);

        private async Task AggregateAndBulkCopyAsync(Model model)
        {
            using var response = await GetModelResponseAsync(ModelUris[(int) model]);
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
                    var tempTable = $"_temp_{_pricesTable}";
                    var modelColumn = ModelColumns[(int) model];

                    var sql = $"CREATE TEMP TABLE {tempTable} (id INT PRIMARY KEY, {modelColumn} JSON) ON COMMIT DROP";
                    await conn.ExecuteNonQueryAsync(sql); // Create temp table

                    // COPY into temp table
                    using (var importer = conn.BeginBinaryImport($"COPY {tempTable} (id, {modelColumn}) FROM STDIN (FORMAT BINARY)"))
                    {
                        ImportFromContentStream(stream, importer);
                        await importer.CompleteAsync();
                    }

                    // Lock table
                    sql = $"LOCK TABLE {_pricesTable} IN ACCESS EXCLUSIVE MODE";
                    await conn.ExecuteNonQueryAsync(sql);

                    // Upsert into main
                    sql = $"INSERT INTO {_pricesTable} (id, {modelColumn}) SELECT * FROM {tempTable} ON CONFLICT (id) DO UPDATE SET id=EXCLUDED.id, {modelColumn}=EXCLUDED.{modelColumn}";

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

        private static void ImportFromContentStream(Stream stream, NpgsqlBinaryImporter importer)
        {
            using var jSr = new Utf8JsonStreamReader(stream, 32 * 1024);
            while (jSr.Read())
            {
                if (jSr.CurrentDepth != 2 || jSr.TokenType != JsonTokenType.PropertyName)
                    continue;

                if (!int.TryParse(jSr.GetString(), out var id))
                {
                    Log.Error("Error"); // TODO
                    continue;
                }

                jSr.Read();
                using var doc = jSr.GetJsonDocument();
                {
                    importer.StartRow();
                    importer.Write(id);
                    importer.Write(doc.RootElement.GetRawText());
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var factory = _dbContextHelper.GetFactory();
            {
                var dbContext = factory.GetDbContext();
                var model = dbContext.Model;
                var entityType = model.FindEntityType(typeof(RealtimePrice));

                Log.Information("{@S}", entityType.GetTableName());

                foreach (var prop in entityType.GetProperties())
                {
                    Log.Information("{@P}", prop.GetColumnBaseName());
                }
            }
            return;

            await AggregateAndBulkCopyLatestAsync();
            await AggregateAndBulkCopyFiveMinuteAsync();
            await AggregateAndBulkCopyOneHourAsync();
        }
    }
}
