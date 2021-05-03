using System;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;

namespace OSItemIndex.Aggregator.Services
{
    public class RealtimePricesRepository : IRealtimePricesRepository
    {
        private readonly IHttpClientFactory _httpFactory;

        public RealtimePricesRepository(IHttpClientFactory factory)
        {
            _httpFactory = factory;
        }

        public async Task<HttpResponseMessage> GetLatestPricesAsync() => await GetResponseAsync(Endpoints.Realtime.PriceLatest);
        public async Task<HttpResponseMessage> GetFiveMinutePricesAsync() => await GetResponseAsync(Endpoints.Realtime.PriceFiveMin);
        public async Task<HttpResponseMessage> GetOneHourPricesAsync() => await GetResponseAsync(Endpoints.Realtime.PriceOneHour);

        private async Task<HttpResponseMessage> GetResponseAsync(string uri)
        {
            var client = _httpFactory.CreateClient("realtimeprices");
            try
            {
                return await client.GetAsync(uri);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to retrieve item prices from realtime-prices api ({$Uri})", uri);
                throw;
            }
        }
    }
}
