﻿using ClassifiedAds.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifiedAds.Persistance.MappingConfigurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            // Seed
            builder.HasData(new List<Product> {
                new Product
                {
                    Id = Guid.Parse("6672E891-0D94-4620-B38A-DBC5B02DA9F7"),
                    Code = "TEST",
                    Name = "Test",
                    Description = "Description"
                },
                new Product
                {
                    Id = Guid.Parse("CC9D7ECA-6428-4E6D-B40B-2C8D93AB7347"),
                    Code = "PD001",
                    Name = "Iphone X",
                    Description = "Iphone X"
                }
            });
        }
    }
}
