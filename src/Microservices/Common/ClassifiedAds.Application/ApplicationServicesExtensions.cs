﻿using System;
using System.Linq;
using System.Reflection;
using ClassifiedAds.Application;
using ClassifiedAds.Domain.Entities;
using ClassifiedAds.Domain.Events;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<Dispatcher>();

            services.AddScoped<IDomainEvents, DomainEvents>()
                .AddScoped(typeof(ICrudService<>), typeof(CrudService<>));

            return services;
        }

        public static IServiceCollection AddMessageHandlers(this IServiceCollection services, Assembly assembly)
        {
            var assemblyTypes = assembly.GetTypes();

            foreach (var type in assemblyTypes)
            {
                var handlerInterfaces = type.GetInterfaces()
                   .Where(Utils.IsHandlerInterface)
                   .ToList();

                if (handlerInterfaces.Any())
                {
                    var handlerFactory = new HandlerFactory(type);
                    foreach (var interfaceType in handlerInterfaces)
                    {
                        services.AddTransient(interfaceType, provider => handlerFactory.Create(provider, interfaceType));
                    }
                }
            }

            var aggregateRootTypes = assembly.GetTypes().Where(x => x.BaseType == typeof(AggregateRoot<Guid>)).ToList();

            var genericHandlerTypes = new[]
            {
                typeof(GetEntititesQueryHandler<>),
                typeof(GetEntityByIdQueryHandler<>),
                typeof(AddOrUpdateEntityCommandHandler<>),
                typeof(DeleteEntityCommandHandler<>),
            };

            foreach (var aggregateRootType in aggregateRootTypes)
            {
                foreach (var genericHandlerType in genericHandlerTypes)
                {
                    var handlerType = genericHandlerType.MakeGenericType(aggregateRootType);
                    var handlerInterfaces = handlerType.GetInterfaces();

                    var handlerFactory = new HandlerFactory(handlerType);
                    foreach (var interfaceType in handlerInterfaces)
                    {
                        services.AddTransient(interfaceType, provider => handlerFactory.Create(provider, interfaceType));
                    }
                }
            }

            return services;
        }
    }
}
