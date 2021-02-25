using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OSItemIndex.Observer.Services;
using Serilog;
using Microsoft.Extensions.Logging;

namespace OSItemIndex.Observer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Serilog logger config
            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .Enrich.WithThreadId()
              .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{ThreadId}] {Message:lj}{NewLine}{Exception}")
              .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // remove default providers
                    logging.AddSerilog(dispose: true); // add serilog
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddHostedService<OSRSBoxService>();
                });
                
    }
}
