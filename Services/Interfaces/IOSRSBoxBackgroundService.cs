using OSItemIndex.API.Models;
using OSItemIndex.Observer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSItemIndex.Observer.Services
{
    public interface IOSRSBoxBackgroundService
    {
        Task<RealtimeMonitoringProject> GetLatestProjectDetailsAsync();
        Task<HashSet<OSRSBoxItem>> GetLatestItemsAsync();
        Task<int> PostItemsAsync(IEnumerable<OSRSBoxItem> items);

        Task<bool> TriggerUpdateAsync();
    }
}
