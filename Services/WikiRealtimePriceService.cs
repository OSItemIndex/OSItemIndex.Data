using Microsoft.Extensions.Hosting;
using OSItemIndex.API.Models;
using OSItemIndex.Observer.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OSItemIndex.Observer.Services
{  
    public class WikiRealtimePriceService : BackgroundService, IWikiRealtimePriceBackgroundService
    {
        private readonly IHttpClientFactory _httpFactory;

        public WikiRealtimePriceService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> DeserializePricingModelAsync<T>(string endPoint) where T : IWikiRealtimePricingModel
        {
            using (var client = _httpFactory.CreateClient("wikiRealtime"))
            using (var request = new HttpRequestMessage(HttpMethod.Get, endPoint))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        HashSet<T> readContent(Stream s)
                        {
                            var lastItemId = -1;
                            long? foundTimestamp = null;
                            var prices = new HashSet<T>();
                            using (var jsonStreamReader = new Utf8JsonStreamReader(s, 32 * 1024))
                            {
                                while (jsonStreamReader.Read())
                                {
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
                                                    var price = jsonStreamReader.Deserialise<T>();

                                                    price.Id = lastItemId;
                                                    lastItemId = -1;

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

                            if (foundTimestamp != null && typeof(T) == typeof(WikiRealtimePrice.AveragePrice))
                            {
                                foreach (var price in prices)
                                    (price as WikiRealtimePrice.AveragePrice).Timestamp = foundTimestamp;
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
            return new HashSet<T>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("WikiRealtimePriceService is starting");

            stoppingToken.Register(() => Log.Information("WikiRealtimePriceService background service is stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {
                var latest = await DeserializePricingModelAsync<WikiRealtimePrice.LatestPrice>(Constants.Endpoints.WikiRealtimePriceLatest);
                var fiveMinute = await DeserializePricingModelAsync<WikiRealtimePrice.AveragePrice>(Constants.Endpoints.WikiRealtimePriceFiveMin);
                var oneHour = await DeserializePricingModelAsync<WikiRealtimePrice.AveragePrice>(Constants.Endpoints.WikiRealtimePriceOneHour);

                await Task.Delay(60000, stoppingToken);
            }

            Log.Information("WikiRealtimePriceService background service is stopping");
        }
    }
}
