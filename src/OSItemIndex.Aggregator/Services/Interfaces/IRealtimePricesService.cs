using System.Threading.Tasks;

namespace OSItemIndex.Aggregator.Services
{
    public enum RealtimeRequest
    {
        Latest, FiveMinute, OneHour
    }

    public interface IRealtimePricesService : IStatefulService
    {
        Task AggregateAsync();
        Task AggregateAsync(RealtimeRequest requestType);
    }
}
