﻿using Azure.Messaging.ServiceBus;
using ClassifiedAds.Domain.Infrastructure.MessageBrokers;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassifiedAds.Infrastructure.MessageBrokers.AzureServiceBus
{
    public class AzureServiceBusTopicSender<T> : IMessageSender<T>
    {
        private readonly string _connectionString;
        private readonly string _topicName;

        public AzureServiceBusTopicSender(string connectionString, string topicName)
        {
            _connectionString = connectionString;
            _topicName = topicName;
        }

        public async Task SendAsync(T message, MetaData metaData, CancellationToken cancellationToken = default)
        {
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusSender sender = client.CreateSender(_topicName);
            var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message<T>
            {
                Data = message,
                MetaData = metaData,
            })));
            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        }
    }
}
