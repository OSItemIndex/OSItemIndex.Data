using Microsoft.Extensions.DependencyInjection;

namespace OSItemIndex.AggregateService
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// TODO ~
        /// </summary>
        /// <param name="httpClientName"></param>
        /// <returns></returns>
        public static IServiceCollection AddOsItemIndexServiceBundle<T>(this IServiceCollection services, string httpClientName) where T : NamedAggregateService
        {
            services.AddTransient<ObserverHttpMessageHandler>();
            services.AddHttpClient(httpClientName).AddHttpMessageHandler<ObserverHttpMessageHandler>();
            services.AddHostedService<T>();
            return services;
        }
    }
}
