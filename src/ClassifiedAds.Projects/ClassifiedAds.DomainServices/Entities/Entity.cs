﻿using System.ComponentModel.DataAnnotations;

namespace ClassifiedAds.DomainServices.Entities
{
    public class Entity<TId> : IHasKey<TId>, ITrackable
    {
        public TId Id { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
