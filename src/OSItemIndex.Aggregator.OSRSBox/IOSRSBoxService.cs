using System.Collections.Generic;
using System.Threading.Tasks;
using OSItemIndex.Aggregator.OSRSBox.Models;
using OSItemIndex.Models;

namespace OSItemIndex.Aggregator.OSRSBox
{
    public interface IOsrsBoxService
    {
        Task<IEnumerable<OsrsBoxItem>> GetItemsAsync();
        Task<ReleaseMonitoringProject> GetReleaseMonitoringProjectAsync();
    }
}
