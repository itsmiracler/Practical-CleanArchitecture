﻿using Azure.Messaging.ServiceBus;
using ClassifiedAds.Domain.Infrastructure.MessageBrokers;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassifiedAds.Infrastructure.MessageBrokers.AzureServiceBus
{
    public class AzureServiceBusSender<T> : IMessageSender<T>
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public AzureServiceBusSender(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task SendAsync(T message, MetaData metaData, CancellationToken cancellationToken = default)
        {
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusSender sender = client.CreateSender(_queueName);
            var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message<T>
            {
                Data = message,
                MetaData = metaData,
            })));
            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        }
    }
}
