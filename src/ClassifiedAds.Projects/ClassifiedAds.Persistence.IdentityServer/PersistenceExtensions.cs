﻿using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClassifiedAds.CrossCuttingConcerns.ExtensionMethods;
using ClassifiedAds.DomainServices.Entities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PersistenceExtensions
    {
        public static IIdentityServerBuilder AddPersistence(this IIdentityServerBuilder services, string connectionString)
        {
            services.AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(typeof(PersistenceExtensions).GetTypeInfo().Assembly.GetName().Name));
                options.DefaultSchema = "idsv";
            })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(typeof(PersistenceExtensions).GetTypeInfo().Assembly.GetName().Name));
                    options.DefaultSchema = "idsv";

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30; // interval in seconds
                })
                .AddAspNetIdentity<User>();
            return services;
        }

        public static void MigrateIdServerDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    var clients = new List<Client> {
                    new Client
                    {
                        ClientId = "ClassifiedAds.WebMVC",
                        ClientName ="ClassifiedAds Web MVC",
                        AllowedGrantTypes = GrantTypes.Hybrid.Combines(GrantTypes.ResourceOwnerPassword),
                        RedirectUris =
                        {
                            "https://localhost:44364/signin-oidc",
                            "http://localhost:9003/signin-oidc",
                        },
                        PostLogoutRedirectUris =
                        {
                            "https://localhost:44364/signout-callback-oidc",
                            "http://localhost:9003/signout-callback-oidc",
                        },
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "ClassifiedAds.WebAPI"
                        },
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },
                        AllowOfflineAccess = true
                    },
                    new Client
                    {
                        ClientId = "spa-client",
                        ClientName ="SPA Client",
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        RedirectUris =
                        {
                            "http://localhost:4200/assets/oidc-login-redirect.html"
                        },
                        PostLogoutRedirectUris =
                        {
                            "http://localhost:4200/?postLogout=true"
                        },
                        AllowedCorsOrigins =
                        {
                            "http://localhost:4200/"
                        },
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "ClassifiedAds.WebAPI"
                        },
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },
                        AllowOfflineAccess = true
                    }
                };

                    foreach (var client in clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    var identityResources = new List<IdentityResource>()
                    {
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile(),
                    };

                    foreach (var resource in identityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    var apiResources = new List<ApiResource>
                    {
                        new ApiResource("ClassifiedAds.WebAPI", "ClassifiedAds Web API",
                        new List<string>() {"role" } )
                    };
                    foreach (var resource in apiResources)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
