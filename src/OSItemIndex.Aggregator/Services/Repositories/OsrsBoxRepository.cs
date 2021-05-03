using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Serilog;

namespace OSItemIndex.Aggregator.Services
{
    public class OsrsBoxRepository : IOsrsBoxRepository
    {
        private readonly IHttpClientFactory _httpFactory;

        public OsrsBoxRepository(IHttpClientFactory factory)
        {
            _httpFactory = factory;
        }

        public async Task<ReleaseMonitoringProject> GetProjectDetailsAsync()
        {
            var client = _httpFactory.CreateClient("osrsbox");
            try
            {
                // GetFromJsonAsync will only return null if there's an exception, which we catch
                return (await client.GetFromJsonAsync<ReleaseMonitoringProject>(Endpoints.OsrsBox.Project))!;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to retrieve and deserialize project details for osrsbox from release-monitoring project");
                throw;
            }
        }

        public async Task<HttpResponseMessage> RequestCompleteItemsAsync()
        {
            var client = _httpFactory.CreateClient("osrsbox");
            try
            {
                return await client.GetAsync(Endpoints.OsrsBox.ItemsComplete);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to retrieve complete items from osrsbox static json api");
                throw;
            }
        }
    }
}
