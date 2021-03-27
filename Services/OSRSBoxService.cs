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
        private readonly IHttpClientFactory httpFactory;

        public OSRSBoxService(IHttpClientFactory _httpFactory)
        {
            httpFactory = _httpFactory;
        }

        public async Task<HashSet<OSRSBoxItem>> GetLatestItemsAsync()
        {
            using (var client = httpFactory.CreateClient("osrsbox"))
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

                            using (var jsonStreamReader = new Utf8JsonStreamReader(stream, 32 * 1024))
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
            using (var client = httpFactory.CreateClient("osrsbox"))
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

        public async Task<int> PostItemsAsync(IEnumerable<OSRSBoxItem> items)
        {
            using (var client = httpFactory.CreateClient("osrsbox"))
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
                    return int.Parse(content);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "StatusCode: {@StatusCode}, Content: {@Content}", response.StatusCode, content);
                }
            }
            return 0;
        }

        public async Task<bool> TriggerUpdateAsync()
        {
            var project = await GetLatestProjectDetailsAsync();
            var items = await GetLatestItemsAsync();

            Log.Information("Latest OSRSBox project details > v{@Version} and {@Count} item records", project.Version, items.Count);
            var itemsAdded = await PostItemsAsync(items);
            return itemsAdded > 0;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("OSRSBoxService is starting");

            stoppingToken.Register(() => Log.Information("OSRSBoxService background service is stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {
                await TriggerUpdateAsync();


                // Delay every 10 minutes
                // Trigger post if
                    // database is empty
                    // database item record amount does not match osrsbox's
                    // osrsbox version has changed

                //Log.Information("Fetched latest osrsbox project details {@Version} - {@Items}", project.Version, items.Count);

                // delay by a day, maybe 2-12 hours if the latest project version is latest
                // trigger if the database is empty
                // trigger if the database number of items doesn't match osrsbox's
                // trigger if the version is different than that of the project details version
                // check version every 2-24 hours
                // check database integrity every hour
                // delay every hour

                await Task.Delay(60000, stoppingToken);
            }

            Log.Information("OSRSBoxService background service is stopping");
        }
    }
}
