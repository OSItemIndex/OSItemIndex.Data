using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace OSItemIndex.AggregateService
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// TODO ~
        /// </summary>
        /// <param name="httpClientName"></param>
        /// <returns></returns>
        public static IServiceCollection AddAggregatorServiceBundle<T>(this IServiceCollection services, string httpClientName) where T : NamedAggregateService
        {
            services.AddTransient<ObserverHttpMessageHandler>();
            services.AddHttpClient(httpClientName).AddHttpMessageHandler<ObserverHttpMessageHandler>();
            services.AddHostedService<T>();
            return services;
        }

        /// <summary>
        /// TODO ~
        /// </summary>
        /// <param name="services"></param>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        public static IServiceCollection AddAggregatorRedisPool(this IServiceCollection services, string keyPrefix = null, DatabaseKeys databaseKey = 0)
        {
            var conf = new RedisConfiguration()
            {
                AbortOnConnectFail = true,
                KeyPrefix = keyPrefix ?? "",
                Hosts = new[] { new RedisHost { Host = "redis", Port = 6379 } },
                AllowAdmin = true,
                ConnectTimeout = 10000,
                Database = (int) databaseKey,
                PoolSize = 50,
                ServerEnumerationStrategy = new ServerEnumerationStrategy()
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                }
            };

            services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(conf);
            return services;
        }
    }
}
