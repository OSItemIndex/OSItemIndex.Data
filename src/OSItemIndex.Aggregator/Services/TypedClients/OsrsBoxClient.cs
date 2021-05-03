using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Serilog;

namespace OSItemIndex.Aggregator.Services
{
    public class OsrsBoxClient : IOsrsBoxClient
    {
        private HttpClient Client { get; }

        public OsrsBoxClient(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("User-Agent", Constants.ObserverUserAgent);
            Client = client;
        }

        public async Task<ReleaseMonitoringProject> GetProjectDetailsAsync()
        {
            try
            {
                // GetFromJsonAsync will only return null if there's an exception, which we catch
                return (await Client.GetFromJsonAsync<ReleaseMonitoringProject>(Endpoints.OsrsBox.Project))!;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to retrieve and deserialize project details for osrsbox from release-monitoring project");
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetRawItemsCompleteAsync()
        {
            try
            {
                return await Client.GetAsync(Endpoints.OsrsBox.ItemsComplete);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to retrieve complete items from osrsbox static json api");
                throw;
            }
        }
    }
}
