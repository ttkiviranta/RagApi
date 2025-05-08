using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagApi.Data;
using RagApi.Models;
using Microsoft.EntityFrameworkCore;
using RagApi.Auth;

namespace RagApi.Api.Auth
{
    /// <summary>
    /// Service for handling Azure Entra ID authentication with caching
    /// </summary>
    public class EntraAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AzureAdOptions _azureAdOptions;
        private readonly ILogger<EntraAuthService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMemoryCache _cache;

        // Cache options
        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(10);
        private const string UserCacheKeyPrefix = "User_";

        /// <summary>
        /// Initializes a new instance of EntraAuthService
        /// </summary>
        public EntraAuthService(
            IHttpContextAccessor httpContextAccessor,
            IOptions<AzureAdOptions> azureAdOptions,
            ILogger<EntraAuthService> logger,
            ApplicationDbContext dbContext,
            IMemoryCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _azureAdOptions = azureAdOptions.Value;
            _logger = logger;
            _dbContext = dbContext;
            _cache = cache;
        }

        /// <summary>
        /// Gets the current user ID from the claims principal
        /// </summary>
        public string? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // Look for the object ID claim from Azure AD
            var userId = user?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ??
                user?.FindFirst("oid")?.Value ??
                user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Could not find user ID in claims");
                return null;
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
                user?.FindFirst("preferred_username")?.Value ?? "Unknown User";
        }

        /// <summary>
        /// Gets the current user's email from the claims principal
        /// </summary>
        public string GetUserEmail()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.FindFirst("preferred_username")?.Value ??
                user?.FindFirst(ClaimTypes.Email)?.Value ??
                user?.FindFirst("email")?.Value ?? "unknown@example.com";
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
            // Check if user is in admin group
            var isInAdminGroup = IsInGroup(_azureAdOptions.Groups?.Admin);
            
            // Also check if user has admin role
            var isInAdminRole = IsInRole("Admin");
            
            return isInAdminGroup || isInAdminRole;
        }

        /// <summary>
        /// Checks if user is in the specified Azure AD group
        /// </summary>
        public bool IsInGroup(string? groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return false;

            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null)
                    return false;

                // Check groups claim
                var groups = user.FindAll("groups")
                    .Select(c => c.Value)
                    .ToList();

                // If groups claim is empty, try the traditional role claim
                if (groups.Count == 0)
                {
                    groups = user.FindAll(ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();
                }

                return groups.Contains(groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is in group {GroupId}", groupId);
                return false;
            }
        }

        /// <summary>
        /// Gets or creates a user in the database based on Entra ID claims
        /// Uses caching to reduce database calls
        /// </summary>
        public async Task<User?> GetOrCreateUserAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Cannot get or create user - User ID is empty");
                return null;
            }

            // Try to get from cache first
            var cacheKey = $"{UserCacheKeyPrefix}{userId}";
            if (_cache.TryGetValue<User>(cacheKey, out var cachedUser))
            {
                _logger.LogDebug("User {UserId} retrieved from cache", userId);
                return cachedUser;
            }

            try
            {
                // Get user from database
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
                    // Update existing user - but don't update on every request
                    // Only update if login time is older than 1 hour or role has changed
                    var shouldUpdate = (DateTime.UtcNow - user.LastLogin).TotalHours >= 1
                        || user.IsAdmin != IsAdmin();

                    if (shouldUpdate)
                    {
                        _logger.LogInformation("Updating existing user with ID {UserId}", userId);

                        user.Username = GetUserName();
                        user.Email = GetUserEmail();
                        user.IsAdmin = IsAdmin();
                        user.LastLogin = DateTime.UtcNow;
                        user.LastRoleSync = DateTime.UtcNow;

                        _dbContext.Users.Update(user);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                // Add to cache
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheExpirationTime)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                
                _cache.Set(cacheKey, user, cacheEntryOptions);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating user {UserId}", userId);
                throw;
            }
        }
    }
}
