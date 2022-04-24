﻿using ClassifiedAds.CrossCuttingConcerns.Csv;
using ClassifiedAds.Domain.Events;
using ClassifiedAds.Domain.Repositories;
using ClassifiedAds.Infrastructure.Csv;
using ClassifiedAds.Modules.Product.ConfigurationOptions;
using ClassifiedAds.Modules.Product.Entities;
using ClassifiedAds.Modules.Product.HostedServices;
using ClassifiedAds.Modules.Product.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProductModuleServiceCollectionExtensions
    {
        public static IServiceCollection AddProductModule(this IServiceCollection services, Action<ProductModuleOptions> configureOptions)
        {
            var settings = new ProductModuleOptions();
            configureOptions(settings);

            services.Configure(configureOptions);

            services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(settings.ConnectionStrings.Default, sql =>
            {
                if (!string.IsNullOrEmpty(settings.ConnectionStrings.MigrationsAssembly))
                {
                    sql.MigrationsAssembly(settings.ConnectionStrings.MigrationsAssembly);
                }
            }));

            services
                .AddScoped<IRepository<Product, Guid>, Repository<Product, Guid>>()
                .AddScoped(typeof(IProductRepository), typeof(ProductRepository))
                .AddScoped<IRepository<AuditLogEntry, Guid>, Repository<AuditLogEntry, Guid>>()
                .AddScoped<IRepository<EventLog, long>, Repository<EventLog, long>>();

            DomainEvents.RegisterHandlers(Assembly.GetExecutingAssembly(), services);

            services.AddMessageHandlers(Assembly.GetExecutingAssembly());

            services.AddAuthorizationPolicies(Assembly.GetExecutingAssembly());

            services.AddScoped(typeof(ICsvReader<>), typeof(CsvReader<>));
            services.AddScoped(typeof(ICsvWriter<>), typeof(CsvWriter<>));

            return services;
        }

        public static IMvcBuilder AddProductModule(this IMvcBuilder builder)
        {
            return builder.AddApplicationPart(Assembly.GetExecutingAssembly());
        }

        public static void MigrateProductDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<ProductDbContext>().Database.Migrate();
            }
        }

        public static IServiceCollection AddHostedServicesProductModule(this IServiceCollection services)
        {
            services.AddScoped<PublishEventService>();
            services.AddHostedService<PublishEventWorker>();

            return services;
        }
    }
}
