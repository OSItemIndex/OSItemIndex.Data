using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OSItemIndex.Aggregator.Extensions;
using OSItemIndex.Aggregator.Services;
using OSItemIndex.Data.Extensions;
using OSItemIndex.Data.HostedServices;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace OSItemIndex.Aggregator
{
    static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                         .Enrich.FromLogContext()
                         .Enrich.WithThreadId()
                         .Enrich.WithExceptionDetails()
                         .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                         .CreateBootstrapLogger();
            CreateHostBuilder(args).Build().Run();
            Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureSerilog()
                       .ConfigureAppConfiguration((hostingContext, config) =>
                       {
                           var env = hostingContext.HostingEnvironment;

                           config.Sources.Clear();
                           config.SetBasePath(env.ContentRootPath);
                           config.AddJsonFile("appsettings.json", true, true);
                           config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);
                           config.AddKeyPerFile("/run/secrets", true); // docker secrets - https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#key-per-file-configuration-provider
                           config.AddEnvironmentVariables();
                       })
                       .ConfigureServices((hostingContext, services) =>
                       {
                           services.AddEntityFrameworkContext(hostingContext.Configuration);
                           services.AddHostedService<DatabaseInitializerService>();
                           services.AddAggregator<OsrsBoxService>("osrsbox");
                       });
        }

        public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); }); // remove default providers
            builder.UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
                                   .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                                   .Enrich.FromLogContext()
                                   .Enrich.WithThreadId()
                                   .Enrich.WithExceptionDetails()
                                   .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")); // {Properties:j}
            return builder;
        }
    }
}
