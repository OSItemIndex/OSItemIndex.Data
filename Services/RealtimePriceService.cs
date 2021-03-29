using Microsoft.Extensions.Hosting;
using OSItemIndex.API.Models;
using OSItemIndex.Observer.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OSItemIndex.Observer.Services
{  
    public class RealtimePriceService : BackgroundService, IRealtimePriceBackgroundService
    {
        private readonly IHttpClientFactory _httpFactory;

        public RealtimePriceService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IEnumerable<RealtimePrice>> DeserializePricingModelAsync(Realtime.RequestType requestType)
        {
            using (var client = _httpFactory.CreateClient("realtime"))
            using (var request = new HttpRequestMessage(HttpMethod.Get, Realtime.RequestEndpoints[requestType]))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        HashSet<RealtimePrice> readContent(Stream s)
                        {
                            var lastItemId = -1;
                            long? foundTimestamp = null;
                            var prices = new HashSet<RealtimePrice>();
                            using (var jsonStreamReader = new Utf8JsonStreamReader(s, 32 * 1024))
                            {
                                while (jsonStreamReader.Read())
                                {
                                    // TODO ~ Write a unit test for confirming the depth
                                    if (jsonStreamReader.CurrentDepth == 2)
                                    {
                                        switch (jsonStreamReader.TokenType)
                                        {
                                            case JsonTokenType.PropertyName: // item id
                                                lastItemId = int.Parse(jsonStreamReader.GetString());
                                                break;
                                            case JsonTokenType.StartObject: // object
                                                if (lastItemId > -1)
                                                {
                                                    var price = new RealtimePrice() { Id = lastItemId };
                                                    lastItemId = -1;
                                                    switch (requestType)
                                                    {
                                                        case Realtime.RequestType.Latest:
                                                            price.Latest = jsonStreamReader.Deserialise<RealtimePrice.LatestPrice>();
                                                            break;

                                                        case Realtime.RequestType.FiveMinute:
                                                            price.FiveMinute = jsonStreamReader.Deserialise<RealtimePrice.AveragePrice>();
                                                            break;

                                                        case Realtime.RequestType.OneHour:
                                                            price.OneHour = jsonStreamReader.Deserialise<RealtimePrice.AveragePrice>();
                                                            break;

                                                        default: break;
                                                    }
                                                    prices.Add(price);
                                                }
                                                break;
                                        }
                                    }
                                    else if (jsonStreamReader.CurrentDepth == 1 && jsonStreamReader.TokenType == JsonTokenType.Number) // timestamp
                                    {
                                        foundTimestamp = jsonStreamReader.GetInt64();
                                    }
                                }
                            }

                            if (foundTimestamp != null)
                            {
                                foreach (var price in prices)
                                {
                                    switch (requestType)
                                    {
                                        case Realtime.RequestType.FiveMinute:
                                            price.FiveMinute.Timestamp = foundTimestamp;
                                            break;

                                        case Realtime.RequestType.OneHour:
                                            price.OneHour.Timestamp = foundTimestamp;
                                            break;

                                        default: break;
                                    }
                                }
                            }

                            return prices;
                        }
                        return readContent(stream);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "StatusCode: {@StatusCode}", response.StatusCode);
                }
            }
            return new HashSet<RealtimePrice>();
        }      

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("RealtimePriceService is starting");

            stoppingToken.Register(() => Log.Information("RealtimePriceService background service is stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {

                await Task.Delay(60000, stoppingToken);
            }

            Log.Information("RealtimePriceService background service is stopping");
        }
    }
}
