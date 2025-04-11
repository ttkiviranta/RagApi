using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagApi.Models;
using RagApi.Data;
using Microsoft.EntityFrameworkCore;

namespace RagApi.Auth
{
    /// <summary>
    /// Service for handling Azure Entra ID authentication
    /// </summary>
    public class EntraAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AzureAdOptions _azureAdOptions;
        private readonly ILogger<EntraAuthService> _logger;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of EntraAuthService
        /// </summary>
        public EntraAuthService(
            IHttpContextAccessor httpContextAccessor,
            IOptions<AzureAdOptions> azureAdOptions,
            ILogger<EntraAuthService> logger,
            ApplicationDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _azureAdOptions = azureAdOptions.Value;
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Gets the current user ID from the claims principal
        /// </summary>
        public string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // Look for the object ID claim from Azure AD
            var userId = user?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ??
                user?.FindFirst("oid")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Could not find user ID in claims");
            }

            return userId;
        }

        /// <summary>
        /// Gets the current user's name from the claims principal
        /// </summary>
        public string GetUserName()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.FindFirst("name")?.Value ??
                user?.FindFirst(ClaimTypes.Name)?.Value ??
                user?.FindFirst("preferred_username")?.Value;
        }

        /// <summary>
        /// Gets the current user's email from the claims principal
        /// </summary>
        public string GetUserEmail()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.FindFirst("preferred_username")?.Value ??
                user?.FindFirst(ClaimTypes.Email)?.Value ??
                user?.FindFirst("email")?.Value;
        }

        /// <summary>
        /// Checks if the current user is in the specified role
        /// </summary>
        public bool IsInRole(string roleName)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.IsInRole(roleName) ?? false;
        }

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        public bool IsAdmin()
        {
            return IsInRole("Admin");
        }

        /// <summary>
        /// Gets or creates a user in the database based on Entra ID claims
        /// </summary>
        public async Task<User> GetOrCreateUserAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Cannot get or create user - User ID is empty");
                return null;
            }

            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    // Create new user
                    _logger.LogInformation("Creating new user with ID {UserId}", userId);

                    user = new User
                    {
                        Id = userId,
                        Username = GetUserName(),
                        Email = GetUserEmail(),
                        IsAdmin = IsAdmin(),
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow,
                        LastRoleSync = DateTime.UtcNow
                    };

                    _dbContext.Users.Add(user);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Update existing user
                    _logger.LogInformation("Updating existing user with ID {UserId}", userId);

                    user.Username = GetUserName();
                    user.Email = GetUserEmail();
                    user.IsAdmin = IsAdmin();
                    user.LastLogin = DateTime.UtcNow;
                    user.LastRoleSync = DateTime.UtcNow;

                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating user");
                throw;
            }
        }
    }
}
