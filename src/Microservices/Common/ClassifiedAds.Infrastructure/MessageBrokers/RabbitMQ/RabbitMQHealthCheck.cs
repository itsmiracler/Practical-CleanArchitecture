﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClassifiedAds.Infrastructure.MessageBrokers.RabbitMQ
{
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly RabbitMQHealthCheckOptions _options;

        public RabbitMQHealthCheck(RabbitMQHealthCheckOptions options)
        {
            _options = options;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionFactory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    UserName = _options.UserName,
                    Password = _options.Password,
                };

                using var connection = connectionFactory.CreateConnection();
                using var model = connection.CreateModel();

                return Task.FromResult(HealthCheckResult.Healthy($"HostName: {_options.HostName}"));
            }
            catch (Exception exception)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, null, exception));
            }
        }
    }

    public class RabbitMQHealthCheckOptions
    {
        public string HostName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
