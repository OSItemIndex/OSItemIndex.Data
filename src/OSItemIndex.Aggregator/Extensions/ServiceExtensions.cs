using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using OSItemIndex.Aggregator.Services;

namespace OSItemIndex.Aggregator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="IHttpClientFactory"/> and a hosted service of type <see cref="T"/> via <see cref="HttpClientFactoryServiceCollectionExtensions.AddHttpClient(IServiceCollection, string)"/>
        /// using the passed <see cref="httpClientName"/> and configures the default headers to use our user-agent.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="httpClientName">A logical name of the <see cref="AggregateService"/> and <see cref="HttpClient"/> to create.</param>
        /// <remarks>User-agent <see cref="Constants.ObserverUserAgent"/></remarks>
        public static IServiceCollection AddAggregator<T>(this IServiceCollection services, string httpClientName) where T : AggregateService
        {
            services.AddHttpClient(httpClientName, client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", Constants.ObserverUserAgent);
            });
            services.AddHostedService<T>();
            return services;
        }
    }
}
