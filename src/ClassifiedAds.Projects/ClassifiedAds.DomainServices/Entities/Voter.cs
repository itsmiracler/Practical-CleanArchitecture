﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifiedAds.DomainServices.Entities
{
    public class Voter : Entity<Guid>
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
    }
}
