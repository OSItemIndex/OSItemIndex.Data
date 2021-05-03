using System.Net.Http;
using System.Threading.Tasks;

namespace OSItemIndex.Aggregator.Services
{
    public interface IRealtimePriceClient
    {
        public Task<HttpResponseMessage> GetRawLatestPricesAsync();
        public Task<HttpResponseMessage> GetRawFiveMinutePricesAsync();
        public Task<HttpResponseMessage> GetRawOneHourPricesAsync();
    }
}
