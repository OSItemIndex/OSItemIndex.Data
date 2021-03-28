using Microsoft.Extensions.Hosting;
using OSItemIndex.API.Models;
using OSItemIndex.Observer.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OSItemIndex.Observer.Utils;

namespace OSItemIndex.Observer.Services
{
    class OSRSBoxService : BackgroundService, IOSRSBoxBackgroundService
    {
        private Version _lastOSRSBoxVersion;
        private int _lastNumberOfItemRecords;
        private readonly IHttpClientFactory _httpFactory;

        public OSRSBoxService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<ItemsStatisics> GetItemStatisticsAsync()
        {
            using (var client = _httpFactory.CreateClient("osrsbox"))
            using (var request = new HttpRequestMessage(HttpMethod.Get, Constants.Endpoints.OSItemIndexAPIStats))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                    return JsonSerializer.Deserialize<ItemsStatisics>(content);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "StatusCode: {@StatusCode}, Content: {@Content}", response.StatusCode, content);
                }
            }
            return new ItemsStatisics();
        }

        public async Task<HashSet<OSRSBoxItem>> GetLatestItemsAsync()
        {
            using (var client = _httpFactory.CreateClient("osrsbox"))
            using (var request = new HttpRequestMessage(HttpMethod.Get, Constants.Endpoints.OSRSBoxItems))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        HashSet<OSRSBoxItem> readItems(Stream s)
                        {
                            var items = new HashSet<OSRSBoxItem>();
                            using (var jsonStreamReader = new Utf8JsonStreamReader(s, 32 * 1024))
                            {
                                while (jsonStreamReader.Read())
                                {
                                    if (jsonStreamReader.CurrentDepth == 1 && jsonStreamReader.TokenType == JsonTokenType.StartObject)
                                    {
                                        var rawJson = jsonStreamReader.DeserialiseAsString();
                                        var item = JsonSerializer.Deserialize<OSRSBoxItem>(rawJson);
                                        item.Document = rawJson;
                                        items.Add(item);
                                    }
                                }
                            }                           
                            return items;
                        }
                        return readItems(stream);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "StatusCode: {@StatusCode}", response.StatusCode);
                }
                return new HashSet<OSRSBoxItem>();
            }
        }

        public async Task<RealtimeMonitoringProject> GetLatestProjectDetailsAsync()
        {
            using (var client = _httpFactory.CreateClient("osrsbox"))
            using (var request = new HttpRequestMessage(HttpMethod.Get, Constants.Endpoints.OSRSBoxVersion))
            using (var response = await client.SendAsync(request))
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                    return JsonSerializer.Deserialize<RealtimeMonitoringProject>(content);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "StatusCode: {@StatusCode}, Content: {@Content}", response.StatusCode, content);
                }
            }
            return new RealtimeMonitoringProject();
        }

        public async Task<bool> PostItemsAsync(IEnumerable<OSRSBoxItem> items)
        {
            using (var client = _httpFactory.CreateClient("osrsbox"))
            using (var request = new HttpRequestMessage(HttpMethod.Post, Constants.Endpoints.OSItemIndexAPIPost)
            {
                Content = JsonContent.Create(items)
            })
            using (var response = await client.SendAsync(request))
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                    var result = int.Parse(content);
                    return result >= 0;
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "StatusCode: {@StatusCode}, Content: {@Content}", response.StatusCode, content);
                }
            }
            return false;
        }

        public async Task<bool> ShouldUpdate()
        {
            var project = await GetLatestProjectDetailsAsync();
            if (Version.Parse(project.Version) != _lastOSRSBoxVersion)
            {
                return true;
            }

            var itemsStats = await GetItemStatisticsAsync();
            if (_lastNumberOfItemRecords != itemsStats.TotalItemRecords)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateItemsAsync()
        {
            Log.Information("Attempting to update OSItemIndex.API items");

            var project = await GetLatestProjectDetailsAsync();
            var items = await GetLatestItemsAsync();
            var posted = await PostItemsAsync(items);

            if (posted)
            {
                Log.Information("Posted items, latest osrsbox information: {@Version} and {@Count} item records", project.Version, items.Count);
                _lastOSRSBoxVersion = Version.Parse(project.Version);
                _lastNumberOfItemRecords = items.Count;
            }
            return posted;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("OSRSBoxService is starting");

            stoppingToken.Register(() => Log.Information("OSRSBoxService background service is stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {               
                if (await ShouldUpdate())
                {
                    await UpdateItemsAsync();                      
                }
                await Task.Delay(30 * 60000, stoppingToken);
            }

            Log.Information("OSRSBoxService background service is stopping");
        }
    }
}
