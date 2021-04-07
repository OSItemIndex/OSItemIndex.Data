using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NReJSON;
using OSItemIndex.AggregateService;
using OSItemIndex.AggregateService.Utils;
using OSItemIndex.Aggregator.OSRSBox.Models;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace OSItemIndex.Aggregator.OSRSBox
{
    public class OsrsBoxService : NamedAggregateService, IOsrsBoxService
    {
        private Stopwatch _timeSinceLastAggregate = null;
        private Version _lastKnownOsrsBoxVersion;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IRedisDatabase _database;

        public OsrsBoxService(IHttpClientFactory httpFactory, IRedisDatabase database) : base("osrsbox", TimeSpan.FromMinutes(10))
        {
            _httpFactory = httpFactory;
            //_database = database;
        }

        public async Task AggregateAndSetItemsAsync()
        {
            Log.Information("{@Name} aggregator service starting aggregation of osrsbox-items", Name);
            using (var client = _httpFactory.CreateClient("osrsbox"))
            using (var request = new HttpRequestMessage(HttpMethod.Get, Endpoints.OsrsBox.ItemsComplete))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                    await Task.Delay(10000); // buffer time for snapshots to debug
                    using(var stream = await response.Content.ReadAsStreamAsync())
                    {
                        ReadContentStream(stream);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "StatusCode: {@StatusCode}", response.StatusCode);
                }
            }
            Log.Information("{@Name} aggregator service finished aggregating osrsbox-items", Name);

            void ReadContentStream(Stream stream)
            {
                using var jSr = new Utf8JsonStreamReader(stream, 32 * 1024);
                while (jSr.Read())
                {
                    if (jSr.CurrentDepth != 1 || jSr.TokenType != JsonTokenType.StartObject)
                        continue;

                    using (var doc = jSr.GetJsonDocument())
                    {
                        var id = doc.RootElement.GetProperty("id").ToString();
                        var str = doc.RootElement.ToString();
                        //_database.Database.JsonSet(id, doc.RootElement.ToString());
                    }
                }
            }
        }

        public async Task<ReleaseMonitoringProject> GetReleaseMonitoringProjectAsync()
        {
            using (var client = _httpFactory.CreateClient("osrsbox"))
            using (var request = new HttpRequestMessage(HttpMethod.Get, Endpoints.OsrsBox.Project))
            using (var response = await client.SendAsync(request))
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                    return JsonSerializer.Deserialize<ReleaseMonitoringProject>(content);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "StatusCode: {@StatusCode}, Content: {@Content}", response.StatusCode,
                              content);
                }
            }

            return new ReleaseMonitoringProject();
        }

        /// <summary>
        ///     Aggregate osrsbox-items into our redis database if
        ///         - We don't know the latest osrsbox-version
        ///         - It's been 12 hours since our last aggregation (To keep the database in-sync)
        ///         - Our redis database has less than 20,000 keys (estimation)
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await AggregateAndSetItemsAsync();
            return;

            var dbSize = (int) await _database.Database.ExecuteAsync("DBSIZE");
            var aggregate = _lastKnownOsrsBoxVersion == null ||
                            _timeSinceLastAggregate.Elapsed >= TimeSpan.FromHours(12) ||
                            dbSize <= 20000;

            // Every hour we check the latest osrsbox pypi version for a change
            if (_lastKnownOsrsBoxVersion == null || _timeSinceLastAggregate.Elapsed >= TimeSpan.FromHours(2))
            {
                var latestProject = await GetReleaseMonitoringProjectAsync();
                var version = new Version(latestProject.Version);
                if (_lastKnownOsrsBoxVersion != version)
                {
                    aggregate = true;
                    _lastKnownOsrsBoxVersion = version;
                }
            }

            if (aggregate)
            {
                Log.Information("{@Name} aggregator service - dbsize: {@Dbsize}",
                                Name,
                                dbSize != -1 ? dbSize : (int) await _database.Database.ExecuteAsync("DBSIZE"));
                Log.Information("{@Name} aggregator service - osrsbox-version: {@Version}", Name, _lastKnownOsrsBoxVersion);
                await AggregateAndSetItemsAsync();
                _timeSinceLastAggregate.Restart();
            }
        }
    }
}
