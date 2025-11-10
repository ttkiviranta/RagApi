using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RagApi.Api.Middleware
{
    /// <summary>
    /// Middleware to handle authentication with SSO integration and development bypass
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the AuthenticationMiddleware
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="configuration">Configuration instance</param>
        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Check for development bypass
                var isDevelopment = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true;
                var enableBypass = _configuration.GetValue<bool>("Development:EnableAuthBypass", false);

                if (isDevelopment && enableBypass)
                {
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (authHeader.StartsWith("Bearer dev-token-bypass"))
                    {
                        // Create development claims for bypass
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "dev-user-123"),
                            new Claim(ClaimTypes.Name, "Development User"),
                            new Claim(ClaimTypes.Email, "dev@localhost.com"),
                            new Claim("preferred_username", "Development User")
                        };

                        var identity = new ClaimsIdentity(claims, "Development");
                        context.User = new ClaimsPrincipal(identity);

                        _logger.LogInformation("Development authentication bypass active for user {UserId}", "dev-user-123");
                    }
                }

                // Additional authentication logging and validation for production
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _logger.LogDebug("Request from authenticated user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in authentication middleware");
            }

            // Continue to the next middleware
            await _next(context);
        }
    }

    /// <summary>
    /// Extension methods for the AuthenticationMiddleware
    /// </summary>
    public static class AuthenticationMiddlewareExtensions
    {
        /// <summary>
        /// Adds the authentication middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseAuthenticationMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}