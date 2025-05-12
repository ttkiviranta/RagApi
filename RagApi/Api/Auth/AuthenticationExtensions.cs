using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace RagApi.Auth
{
    /// <summary>
    /// Extensions for authentication services
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Adds Azure AD authentication to the service collection
        /// </summary>
        public static IServiceCollection AddAzureAdAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure Azure AD options
            services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));

            // Add authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var tenantId = configuration["AzureAd:TenantId"];

                    options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
                    options.Audience = $"api://{configuration["AzureAd:ClientId"]}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuers = new[]
                        {
                            $"https://sts.windows.net/{tenantId}/",
                            $"https://login.microsoftonline.com/{tenantId}/",
                            $"https://login.microsoftonline.com/{tenantId}/v2.0"
                        },
                        RoleClaimType = "role",
                        NameClaimType = "preferred_username"
                    };

                    // Configure JWT bearer events
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger("AzureAdAuthentication");

                            logger.LogInformation("Token validation started");

                            var identity = context.Principal.Identity as ClaimsIdentity;
                            if (identity == null)
                            {
                                logger.LogWarning("Identity is null in token validation");
                                return;
                            }

                            // Log all claims for debugging
                            foreach (var claim in context.Principal.Claims)
                            {
                                logger.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
                            }

                            // Get groups from token
                            var groups = context.Principal.Claims
                                .Where(c => c.Type == "groups" ||
                                            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/groups" ||
                                            c.Type == "http://schemas.microsoft.com/claims/groups")
                                .Select(c => c.Value)
                                .ToList();

                            logger.LogInformation("Found {Count} groups in token", groups.Count);

                            // Map groups to roles
                            var adminGroupId = configuration["AzureAd:Groups:Admin"];
                            var userGroupId = configuration["AzureAd:Groups:User"];

                            if (groups.Contains(adminGroupId))
                            {
                                logger.LogInformation("Adding Admin role");
                                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                            }

                            if (groups.Contains(userGroupId))
                            {
                                logger.LogInformation("Adding User role");
                                identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                            }
                        },

                        OnAuthenticationFailed = context =>
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger("AzureAdAuthentication");

                            logger.LogError(context.Exception, "Authentication failed");
                            return Task.CompletedTask;
                        },

                        OnChallenge = context =>
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger("AzureAdAuthentication");

                            logger.LogWarning("Challenge issued: {Error} - {Description}",
                                context.Error, context.ErrorDescription);
                            return Task.CompletedTask;
                        },

                        OnMessageReceived = context =>
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger("AzureAdAuthentication");

                            logger.LogDebug("JWT bearer message received");
                            return Task.CompletedTask;
                        }
                    };
                });

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("RequireUserRole", policy =>
                    policy.RequireRole("User"));
            });

            return services;
        }
    }
}