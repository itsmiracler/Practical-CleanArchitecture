﻿namespace ClassifiedAds.WebMVC.ConfigurationOptions.MessageBroker
{
    public class RabbitMQOptions
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ExchangeName { get; set; }
        public string RoutingKey_FileUploaded { get; set; }
        public string RoutingKey_FileDeleted { get; set; }
    }
}
