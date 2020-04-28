﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ClassifiedAds.DomainServices.DomainEvents
{
    public static class DomainEvents
    {
        private static List<Type> _handlers = new List<Type>();
        private static IServiceProvider _serviceProvider;

        public static void RegisterHandlers(Assembly assembly, IServiceProvider serviceProvider)
        {
            var types = assembly.GetTypes()
                                .Where(x => x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                                .ToList();

            _handlers.AddRange(types);
            _serviceProvider = serviceProvider;
        }

        public static void Dispatch(IDomainEvent domainEvent)
        {
            foreach (Type handlerType in _handlers)
            {
                bool canHandleEvent = handlerType.GetInterfaces()
                    .Any(x => x.IsGenericType
                        && x.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                        && x.GenericTypeArguments[0] == domainEvent.GetType());

                if (canHandleEvent)
                {
                    dynamic handler = _serviceProvider.GetService(handlerType);
                    handler.Handle((dynamic)domainEvent);
                }
            }
        }
    }
}
