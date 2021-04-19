using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Npgsql;

namespace OSItemIndex.Aggregator.Services
{
    public interface IOsrsBoxService
    {
        /// <summary>Calls the release-monitoring.org api to GET the pypi osrsbox project details - https://release-monitoring.org/api</summary>
        Task<ReleaseMonitoringProject?> GetReleaseMonitoringProjectAsync();
        /// <summary> </summary>
        Task<HttpResponseMessage> GetFullItemsSummaryResponseAsync();
        Task AggregateAndBulkCopyItemsAsync();
    }
}
