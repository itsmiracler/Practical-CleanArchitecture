﻿using ClassifiedAds.Domain.Infrastructure.MessageBrokers;
using ClassifiedAds.Infrastructure.MessageBrokers;
using ClassifiedAds.Infrastructure.MessageBrokers.AzureEventGrid;
using ClassifiedAds.Infrastructure.MessageBrokers.AzureEventHub;
using ClassifiedAds.Infrastructure.MessageBrokers.AzureQueue;
using ClassifiedAds.Infrastructure.MessageBrokers.AzureServiceBus;
using ClassifiedAds.Infrastructure.MessageBrokers.Fake;
using ClassifiedAds.Infrastructure.MessageBrokers.Kafka;
using ClassifiedAds.Infrastructure.MessageBrokers.RabbitMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessageBrokersCollectionExtensions
    {
        public static IServiceCollection AddAzureEventGridSender<T>(this IServiceCollection services, AzureEventGridOptions options)
        {
            services.AddSingleton<IMessageSender<T>>(new AzureEventGridSender<T>(
                                options.DomainEndpoint,
                                options.DomainKey,
                                options.Topics[typeof(T).Name]));
            return services;
        }

        public static IServiceCollection AddAzureEventHubSender<T>(this IServiceCollection services, AzureEventHubOptions options)
        {
            services.AddSingleton<IMessageSender<T>>(new AzureEventHubSender<T>(
                                options.ConnectionString,
                                options.Hubs[typeof(T).Name]));
            return services;
        }

        public static IServiceCollection AddAzureEventHubReceiver<T>(this IServiceCollection services, AzureEventHubOptions options)
        {
            services.AddTransient<IMessageReceiver<T>>(x => new AzureEventHubReceiver<T>(
                                options.ConnectionString,
                                options.Hubs[typeof(T).Name],
                                options.StorageConnectionString,
                                options.StorageContainerNames[typeof(T).Name]));
            return services;
        }

        public static IServiceCollection AddAzureQueueSender<T>(this IServiceCollection services, AzureQueueOptions options)
        {
            services.AddSingleton<IMessageSender<T>>(new AzureQueueSender<T>(
                                options.ConnectionString,
                                options.QueueNames[typeof(T).Name]));
            return services;
        }

        public static IServiceCollection AddAzureQueueReceiver<T>(this IServiceCollection services, AzureQueueOptions options)
        {
            services.AddTransient<IMessageReceiver<T>>(x => new AzureQueueReceiver<T>(
                                options.ConnectionString,
                                options.QueueNames[typeof(T).Name]));
            return services;
        }

        public static IServiceCollection AddAzureServiceBusSender<T>(this IServiceCollection services, AzureServiceBusOptions options)
        {
            services.AddSingleton<IMessageSender<T>>(new AzureServiceBusSender<T>(
                                options.ConnectionString,
                                options.QueueNames[typeof(T).Name]));
            return services;
        }

        public static IServiceCollection AddAzureServiceBusReceiver<T>(this IServiceCollection services, AzureServiceBusOptions options)
        {
            services.AddTransient<IMessageReceiver<T>>(x => new AzureServiceBusReceiver<T>(
                                options.ConnectionString,
                                options.QueueNames[typeof(T).Name]));
            return services;
        }

        public static IServiceCollection AddFakeSender<T>(this IServiceCollection services)
        {
            services.AddSingleton<IMessageSender<T>>(new FakeSender<T>());
            return services;
        }

        public static IServiceCollection AddFakeReceiver<T>(this IServiceCollection services)
        {
            services.AddTransient<IMessageReceiver<T>>(x => new FakeReceiver<T>());
            return services;
        }

        public static IServiceCollection AddKafkaSender<T>(this IServiceCollection services, KafkaOptions options)
        {
            services.AddSingleton<IMessageSender<T>>(new KafkaSender<T>(options.BootstrapServers, options.Topics[typeof(T).Name]));
            return services;
        }

        public static IServiceCollection AddKafkaReceiver<T>(this IServiceCollection services, KafkaOptions options)
        {
            services.AddTransient<IMessageReceiver<T>>(x => new KafkaReceiver<T>(options.BootstrapServers,
                options.Topics[typeof(T).Name],
                options.GroupId));
            return services;
        }

        public static IServiceCollection AddRabbitMQSender<T>(this IServiceCollection services, RabbitMQOptions options)
        {
            services.AddSingleton<IMessageSender<T>>(new RabbitMQSender<T>(new RabbitMQSenderOptions
            {
                HostName = options.HostName,
                UserName = options.UserName,
                Password = options.Password,
                ExchangeName = options.ExchangeName,
                RoutingKey = options.RoutingKeys[typeof(T).Name],
            }));
            return services;
        }

        public static IServiceCollection AddRabbitMQReceiver<T>(this IServiceCollection services, RabbitMQOptions options)
        {
            services.AddTransient<IMessageReceiver<T>>(x => new RabbitMQReceiver<T>(new RabbitMQReceiverOptions
            {
                HostName = options.HostName,
                UserName = options.UserName,
                Password = options.Password,
                ExchangeName = options.ExchangeName,
                RoutingKey = options.RoutingKeys[typeof(T).Name],
                QueueName = options.QueueNames[typeof(T).Name],
                AutomaticCreateEnabled = true,
            }));
            return services;
        }

        public static IServiceCollection AddMessageBusSender<T>(this IServiceCollection services, MessageBrokerOptions options, IHealthChecksBuilder healthChecksBuilder = null, HashSet<string> checkDulicated = null)
        {
            if (options.UsedRabbitMQ())
            {
                services.AddRabbitMQSender<T>(options.RabbitMQ);

                if (healthChecksBuilder != null)
                {
                    var name = "Message Broker (RabbitMQ)";

                    if (checkDulicated == null || !checkDulicated.Contains(name))
                    {
                        healthChecksBuilder.AddRabbitMQ(
                            rabbitConnectionString: options.RabbitMQ.ConnectionString,
                            name: name,
                            failureStatus: HealthStatus.Degraded);
                    }

                    checkDulicated?.Add(name);
                }
            }
            else if (options.UsedKafka())
            {
                services.AddKafkaSender<T>(options.Kafka);

                if (healthChecksBuilder != null)
                {
                    var name = "Message Broker (Kafka)";

                    if (checkDulicated == null || !checkDulicated.Contains(name))
                    {
                        healthChecksBuilder.AddKafka(
                            setup =>
                            {
                                setup.BootstrapServers = options.Kafka.BootstrapServers;
                                setup.MessageTimeoutMs = 5000;
                            },
                            name: name,
                            failureStatus: HealthStatus.Degraded);
                    }

                    checkDulicated?.Add(name);
                }
            }
            else if (options.UsedAzureQueue())
            {
                services.AddAzureQueueSender<T>(options.AzureQueue);

                if (healthChecksBuilder != null)
                {
                    healthChecksBuilder.AddAzureQueueStorage(connectionString: options.AzureQueue.ConnectionString,
                        queueName: options.AzureQueue.QueueNames[typeof(T).Name],
                        name: $"Message Broker (Azure Queue) {typeof(T).Name}",
                        failureStatus: HealthStatus.Degraded);
                }

            }
            else if (options.UsedAzureServiceBus())
            {
                services.AddAzureServiceBusSender<T>(options.AzureServiceBus);

                if (healthChecksBuilder != null)
                {
                    healthChecksBuilder.AddAzureServiceBusQueue(
                        connectionString: options.AzureServiceBus.ConnectionString,
                        queueName: options.AzureServiceBus.QueueNames[typeof(T).Name],
                        name: $"Message Broker (Azure Service Bus) {typeof(T).Name}",
                        failureStatus: HealthStatus.Degraded);
                }

            }
            else if (options.UsedAzureEventGrid())
            {
                services.AddAzureEventGridSender<T>(options.AzureEventGrid);

                // TODO: Add Health Check
            }
            else if (options.UsedAzureEventHub())
            {
                services.AddAzureEventHubSender<T>(options.AzureEventHub);

                // TODO: Add Health Check
            }
            else if (options.UsedFake())
            {
                services.AddFakeSender<T>();
            }

            return services;
        }

        public static IServiceCollection AddMessageBusReceiver<T>(this IServiceCollection services, MessageBrokerOptions options)
        {
            if (options.UsedRabbitMQ())
            {
                services.AddRabbitMQReceiver<T>(options.RabbitMQ);
            }
            else if (options.UsedKafka())
            {
                services.AddKafkaReceiver<T>(options.Kafka);
            }
            else if (options.UsedAzureQueue())
            {
                services.AddAzureQueueReceiver<T>(options.AzureQueue);
            }
            else if (options.UsedAzureServiceBus())
            {
                services.AddAzureServiceBusReceiver<T>(options.AzureServiceBus);
            }
            else if (options.UsedAzureEventHub())
            {
                services.AddAzureEventHubReceiver<T>(options.AzureEventHub);
            }
            else if (options.UsedFake())
            {
                services.AddFakeReceiver<T>();
            }

            return services;
        }
    }
}
