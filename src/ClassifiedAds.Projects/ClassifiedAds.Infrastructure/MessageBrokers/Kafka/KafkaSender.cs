﻿using ClassifiedAds.DomainServices.Infrastructure.MessageBrokers;
using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ClassifiedAds.Infrastructure.MessageBrokers.Kafka
{
    public class KafkaSender<T> : IMessageSender<T>, IDisposable
    {
        private readonly string _topic;
        private readonly IProducer<Null, string> _producer;

        public KafkaSender(string bootstrapServers, string topic)
        {
            _topic = topic;

            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }

        public void Send(T message)
        {
            SendAsync(message).Wait();
        }

        private async Task SendAsync(T message)
        {
            _ = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = JsonConvert.SerializeObject(message) });
        }
    }
}
