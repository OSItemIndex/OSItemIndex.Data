using Microsoft.Extensions.Hosting;
using OSItemIndex.AggregateService;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace OSItemIndex.Aggregator.OSRSBox
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
                         .WriteTo.Console(outputTemplate:
                                          "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                         .CreateBootstrapLogger();

            HostBuilderExtensions.BuildBasicServiceHost<Startup>(args).Run();
            Log.CloseAndFlush();
        }
    }
}
