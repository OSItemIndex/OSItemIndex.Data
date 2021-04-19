using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OSItemIndex.Data.Database;

namespace OSItemIndex.Aggregator.Services
{
    public class PricesRealtimeService : AggregateService, IPricesRealtimeService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IDbContextHelper _dbContextHelper;

        public PricesRealtimeService(IHttpClientFactory httpFactory, IDbContextHelper dbContextHelper) : base("prices-realtime", TimeSpan.FromMinutes(1))
        {
            _httpFactory = httpFactory;
            _dbContextHelper = dbContextHelper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
