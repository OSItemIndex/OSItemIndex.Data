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
        Task<ItemsStatisics> GetItemStatisticsAsync();
        Task<RealtimeMonitoringProject> GetLatestProjectDetailsAsync();
        Task<HashSet<OSRSBoxItem>> GetLatestItemsAsync();
        Task<bool> PostItemsAsync(IEnumerable<OSRSBoxItem> items);

        Task<bool> ShouldUpdate();
        Task<bool> UpdateItemsAsync();      
    }
}
