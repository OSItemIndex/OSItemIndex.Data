using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OsItemIndex.Data.Database;

namespace OsItemIndex.Data.Extensions
{
    public static class DatabaseExtensions
    {
        /// <summary>
        ///
        /// </summary>
        public static IServiceCollection AddEntityFrameworkContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<DbOptions>(configuration);

            var options = configuration.Get<DbOptions>();

            services.AddDbContextPool<OsItemIndexDbContext>(builder =>
                builder.UseNpgsql(options.DbConnectionString), options.PoolSize);

            services.AddSingleton<IDbContextHelper, DbContextHelper>();
            return services;
        }

        /// <summary>
        ///
        /// </summary>
        public static IApplicationBuilder InitializeDatabases(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                try
                {
                    var dbContextHelper = scope.ServiceProvider.GetRequiredService<IDbContextHelper>();
                    using (var factory = dbContextHelper.GetFactory())
                    using (var context = factory.GetDbContext())
                    {
                        context.Database.EnsureCreated(); // TODO ~ Look into migrations
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return app;
        }
    }
}
