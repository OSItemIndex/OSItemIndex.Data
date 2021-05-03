using System;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;

namespace OSItemIndex.Aggregator.Services
{
    public class RealtimePriceClient : IRealtimePriceClient
    {
        private HttpClient Client { get; }

        public RealtimePriceClient(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("User-Agent", Constants.ObserverUserAgent);
            Client = client;
        }

        public async Task<HttpResponseMessage> GetRawLatestPricesAsync() => await GetRawResponseAsync(Endpoints.Realtime.PriceLatest);
        public async Task<HttpResponseMessage> GetRawFiveMinutePricesAsync() => await GetRawResponseAsync(Endpoints.Realtime.PriceFiveMin);
        public async Task<HttpResponseMessage> GetRawOneHourPricesAsync() => await GetRawResponseAsync(Endpoints.Realtime.PriceOneHour);

        private async Task<HttpResponseMessage> GetRawResponseAsync(string uri)
        {
            try
            {
                return await Client.GetAsync(uri);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to retrieve item prices from realtime-prices api ({$Uri})", uri);
                throw;
            }
        }
    }
}
