using System.Net.Http;
using System.Threading.Tasks;

namespace OSItemIndex.Aggregator.Services
{
    public interface IOsrsBoxRepository
    {
        Task<ReleaseMonitoringProject> GetProjectDetailsAsync();
        Task<HttpResponseMessage> RequestCompleteItemsAsync();
    }
}
