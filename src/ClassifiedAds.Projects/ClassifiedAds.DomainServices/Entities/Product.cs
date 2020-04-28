﻿using System;

namespace ClassifiedAds.DomainServices.Entities
{
    public class Product : AggregateRoot<Guid>
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
