﻿using System.Collections.Generic;
using System.Linq;
using ClassifiedAds.CrossCuttingConcerns.ExtensionMethods;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdServerPersistenceExtensions
    {
        public static IIdentityServerBuilder AddIdServerPersistence(this IIdentityServerBuilder services, string connectionString, string migrationsAssembly = "")
        {
            services.AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql =>
                        {
                            if (!string.IsNullOrEmpty(migrationsAssembly))
                            {
                                sql.MigrationsAssembly(migrationsAssembly);
                            }
                        });
                options.DefaultSchema = "idsv";
            })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql =>
                            {
                                if (!string.IsNullOrEmpty(migrationsAssembly))
                                {
                                    sql.MigrationsAssembly(migrationsAssembly);
                                }
                            });
                    options.DefaultSchema = "idsv";

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30; // interval in seconds
                });
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
                    var clients = new List<Client>
                    {
                        new Client
                        {
                            ClientId = "ClassifiedAds.WebMVC",
                            ClientName = "ClassifiedAds Web MVC",
                            AllowedGrantTypes = GrantTypes.Hybrid.Combines(GrantTypes.ResourceOwnerPassword),
                            RedirectUris =
                            {
                                "https://localhost:44364/signin-oidc",
                                "http://host.docker.internal:9003/signin-oidc",
                            },
                            PostLogoutRedirectUris =
                            {
                                "https://localhost:44364/signout-callback-oidc",
                                "http://host.docker.internal:9003/signout-callback-oidc",
                            },
                            AllowedScopes =
                            {
                                IdentityServerConstants.StandardScopes.OpenId,
                                IdentityServerConstants.StandardScopes.Profile,
                                "ClassifiedAds.WebAPI",
                            },
                            ClientSecrets =
                            {
                                new Secret("secret".Sha256()),
                            },
                            AllowOfflineAccess = true,
                        },
                        new Client
                        {
                            ClientId = "ClassifiedAds.BlazorServerSide",
                            ClientName = "ClassifiedAds Blazor Server Side",
                            AllowedGrantTypes = GrantTypes.Hybrid.Combines(GrantTypes.ResourceOwnerPassword),
                            RedirectUris =
                            {
                                "https://localhost:44331/signin-oidc",
                                "http://host.docker.internal:9008/signin-oidc",
                            },
                            PostLogoutRedirectUris =
                            {
                                "https://localhost:44331/signout-callback-oidc",
                                "http://host.docker.internal:9008/signout-callback-oidc",
                            },
                            AllowedScopes =
                            {
                                IdentityServerConstants.StandardScopes.OpenId,
                                IdentityServerConstants.StandardScopes.Profile,
                                "ClassifiedAds.WebAPI",
                            },
                            ClientSecrets =
                            {
                                new Secret("secret".Sha256()),
                            },
                            AllowOfflineAccess = true,
                        },
                        new Client
                        {
                            ClientId = "spa-client",
                            ClientName = "SPA Client",
                            AllowedGrantTypes = GrantTypes.Implicit,
                            AllowAccessTokensViaBrowser = true,
                            RedirectUris =
                            {
                                "http://localhost:4200/assets/oidc-login-redirect.html",
                            },
                            PostLogoutRedirectUris =
                            {
                                "http://localhost:4200/?postLogout=true",
                            },
                            AllowedCorsOrigins =
                            {
                                "http://localhost:4200/",
                            },
                            AllowedScopes =
                            {
                                IdentityServerConstants.StandardScopes.OpenId,
                                IdentityServerConstants.StandardScopes.Profile,
                                "ClassifiedAds.WebAPI",
                            },
                            ClientSecrets =
                            {
                                new Secret("secret".Sha256()),
                            },
                            AllowOfflineAccess = true,
                        },
                    };

                    context.Clients.AddRange(clients.Select(x => x.ToEntity()));

                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    var identityResources = new List<IdentityResource>()
                    {
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile(),
                    };

                    context.IdentityResources.AddRange(identityResources.Select(x => x.ToEntity()));

                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    var apiResources = new List<ApiResource>
                    {
                        new ApiResource("ClassifiedAds.WebAPI", "ClassifiedAds Web API",
                        new List<string>() { "role" }),
                    };

                    context.ApiResources.AddRange(apiResources.Select(x => x.ToEntity()));

                    context.SaveChanges();
                }
            }
        }
    }
}
