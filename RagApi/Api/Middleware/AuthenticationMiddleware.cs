using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RagApi.Api.Middleware
{
    /// <summary>
    /// Middleware to handle authentication with SSO integration
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthenticationMiddleware
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger instance</param>
        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Additional authentication logging and validation can be done here
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