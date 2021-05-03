using System.Threading.Tasks;

namespace OSItemIndex.Aggregator.Services
{
    public interface IOsrsBoxService : IStatefulService
    {
        Task AggregateAsync();
    }
}
