using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace OSItemIndex.AggregateService
{
    public static class HostBuilderExtensions
    {
        public static IHost BuildBasicServiceHost<TStartup>(string[] args)  where TStartup : class
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureForSerilog()
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                           webBuilder.UseStartup<TStartup>();
                       })
                       .Build();
        }

        public static IHostBuilder ConfigureForSerilog(this IHostBuilder builder)
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); }); // remove default providers
            builder.UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
                                   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                   .Enrich.FromLogContext()
                                   .Enrich.WithThreadId()
                                   .Enrich.WithExceptionDetails()
                                   .WriteTo.Console(outputTemplate:
                                                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")); // {Properties:j}
            return builder;
        }
    }
}
