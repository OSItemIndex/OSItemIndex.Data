using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Exceptions;
using OSItemIndex.Observer.Services;
using Serilog.Events;

namespace OSItemIndex.Observer
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // remove default providers
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        //.Enrich.FromLogContext()
                        .Enrich.WithThreadId()
                        .Enrich.WithExceptionDetails()
                        .Enrich.FromLogContext()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}") // {Properties:j}
                        .CreateLogger();
                    logging.AddSerilog(Log.Logger, dispose: true);
                })

                .ConfigureServices((hostContext, services) =>
                {
                    // Use our own HttpMessageHandler so we can intercept and add any overrides that should be applied to all our requests, like our User-Agent
                    services.AddTransient<ObserverHttpMessageHandler>();
                    services.AddHttpClient("osrsbox").AddHttpMessageHandler<ObserverHttpMessageHandler>();

                    services.AddHostedService<OSRSBoxService>();
                });
        }
    }
}
