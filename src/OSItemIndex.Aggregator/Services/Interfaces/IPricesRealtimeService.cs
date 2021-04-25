using System.Net.Http;
using System.Threading.Tasks;

namespace OSItemIndex.Aggregator.Services
{
    public interface IPricesRealtimeService
    {
        public Task<HttpResponseMessage> GetLatestPricesAsync();
        public Task<HttpResponseMessage> GetFiveMinutePricesAsync();
        public Task<HttpResponseMessage> GetOneHourPricesAsync();

        public Task AggregateAndBulkCopyLatestAsync();
        public Task AggregateAndBulkCopyFiveMinuteAsync();
        public Task AggregateAndBulkCopyOneHourAsync();
    }
}
