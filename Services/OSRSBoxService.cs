using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OSItemIndex.Observer.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSItemIndex.Observer.Services
{
    class OSRSBoxService : BackgroundService
    {
        private readonly ILogger<OSRSBoxService> _logger;
        private readonly IHttpClientFactory _httpFactory;

        public OSRSBoxService(ILogger<OSRSBoxService> log, IHttpClientFactory httpFactory)
        {
            _logger = log;
            _httpFactory = httpFactory;
        }

        public async Task<RMProject> GetLatestProjectDetails()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, Constants.Endpoints.OSRSBoxVersion);
            var client = _httpFactory.CreateClient("osrsbox");

            _logger.LogInformation("Sending osrsbox-version request");
            var resp = await client.SendAsync(req);

            try
            {
                resp.EnsureSuccessStatusCode(); // throw exception
            } catch (HttpRequestException e)
            {
                _logger.LogError("HttpRequest exception {@ExceptionStr}", e.ToString());
                _logger.LogDebug("HttpRequest exception {@StatusCode}, {@ReasonPhrase}, {@ReqMessage}", resp.StatusCode, resp.ReasonPhrase, resp.RequestMessage);
                return new RMProject();
            }
            
            _logger.LogInformation("Response recieved {@StatusCode}", resp.StatusCode);
            var content = await resp.Content.ReadAsStringAsync();
            try
            {
                var project = JsonConvert.DeserializeObject<RMProject>(content);
                return project;
            } catch (JsonException e)
            {
                _logger.LogError("JsonException thrown {@ExceptionStr}", e.ToString());
                _logger.LogDebug("JsonException content {@Content}", content);
            }
            return new RMProject();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OSRSBoxService is starting");

            stoppingToken.Register(() => _logger.LogInformation("OSRSBoxService background service is stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {
                var project = await GetLatestProjectDetails();

                // delay by a day, maybe 2-12 hours if the latest project version is latest
                // trigger if the database is empty
                // trigger if the database number of items doesn't match osrsbox's
                // trigger if the version is different than that of the project details version
                // check version every 2-24 hours
                // check database integrity every hour
                // delay every hour

                await Task.Delay(60000, stoppingToken);
            }

            _logger.LogInformation("OSRSBoxService background service is stopping"));

            /*logger.LogInformation($"GracePeriodManagerService is starting.");

            stoppingToken.Register(() => logger.LogInformation($" GracePeriod background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation($"GracePeriod task doing background work.");

                await Task.Delay(60000, stoppingToken);
            }

            logger.LogInformation($"GracePeriod background task is stopping.");*/
        }
    }
}
