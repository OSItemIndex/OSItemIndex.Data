using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OSItemIndex.AggregateService;
using OSItemIndex.Aggregator.OSRSBox.Models;
using OSItemIndex.Models;

namespace OSItemIndex.Aggregator.OSRSBox
{
    public class OsrsBoxService : NamedAggregateService, IOsrsBoxService
    {
        private readonly IHttpClientFactory _httpFactory;

        public OsrsBoxService(IHttpClientFactory httpFactory) : base("osrsbox", TimeSpan.FromMinutes(10))
        {
            _httpFactory = httpFactory;
        }

        public async Task<IEnumerable<OsrsBoxItem>> GetItemsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ReleaseMonitoringProject> GetReleaseMonitoringProjectAsync()
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
