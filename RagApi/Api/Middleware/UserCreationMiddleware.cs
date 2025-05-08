using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RagApi.Api.Auth;

namespace RagApi.Api.Middleware
{
    /// <summary>
    /// Middleware that automatically creates or updates users in the database
    /// based on the authenticated user's identity from Azure AD
    /// </summary>
    public class UserCreationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserCreationMiddleware> _logger;

        public UserCreationMiddleware(RequestDelegate next, ILogger<UserCreationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, EntraAuthService authService)
        {
            // Only for authenticated users
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    // Get or create user in the database (uses caching internally)
                    var user = await authService.GetOrCreateUserAsync();
                    if (user != null)
                    {
                        _logger.LogDebug("User {UserId} processed by middleware", user.Id);
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the request if user creation has issues
                    _logger.LogError(ex, "Error in user creation middleware");
                }
            }

            // Continue to the next middleware
            await _next(context);
        }
    }

    /// <summary>
    /// Extension methods for the UserCreationMiddleware
    /// </summary>
    public static class UserCreationMiddlewareExtensions
    {
        /// <summary>
        /// Adds the middleware that creates/updates user records from authentication
        /// </summary>
        public static IApplicationBuilder UseUserCreation(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserCreationMiddleware>();
        }
    }
}
