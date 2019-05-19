﻿using ClassifiedAds.Domain.Entities;
using ClassifiedAds.Identity;
using ClassifiedAds.Persistence;
using Microsoft.AspNetCore.Identity;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
                    {
                        options.Tokens.EmailConfirmationTokenProvider = "EmailConfirmation";
                    })
                    .AddEntityFrameworkStores<AdsDbContext>()
                    .AddDefaultTokenProviders()
                    .AddTokenProvider<EmailConfirmationTokenProvider<User>>("EmailConfirmation");

            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(3));
            services.Configure<EmailConfirmationTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromDays(2));
            return services;
        }
    }
}
