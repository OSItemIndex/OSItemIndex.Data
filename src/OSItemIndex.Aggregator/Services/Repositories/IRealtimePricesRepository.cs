using System.Net.Http;
using System.Threading.Tasks;

namespace OSItemIndex.Aggregator.Services
{
    public interface IRealtimePricesRepository
    {
        public Task<HttpResponseMessage> GetLatestPricesAsync();
        public Task<HttpResponseMessage> GetFiveMinutePricesAsync();
        public Task<HttpResponseMessage> GetOneHourPricesAsync();
    }
}
