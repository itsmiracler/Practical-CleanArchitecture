﻿using ClassifiedAds.DomainServices;
using ClassifiedAds.DomainServices.Repositories;
using ClassifiedAds.Persistence;
using ClassifiedAds.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PersistanceExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<AdsDbContext>(options => options.UseSqlServer(connectionString, sql =>
            {
                if (!string.IsNullOrEmpty(migrationsAssembly))
                {
                    sql.MigrationsAssembly(migrationsAssembly);
                }
            }))
                    .AddScoped<IUnitOfWork, UnitOfWork>()
                    .AddScoped(typeof(IRepository<>), typeof(Repository<>));
            return services;
        }

        public static void MigrateDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<AdsDbContext>().Database.Migrate();
            }
        }
    }
}
