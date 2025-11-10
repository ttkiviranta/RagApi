// UserService.cs - Implementation of the IUserService interface for user management
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RagApi.Api.Models;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IRequestContext _requestContext;

        /// <summary>
        /// Initializes a new instance of the UserService class
        /// </summary>
        /// <param name="unitOfWork">Unit of work for database operations</param>
        /// <param name="logger">Logger for service operations</param>
        /// <param name="requestContext">Request context for accessing user claims</param>
        public UserService(
            IUnitOfWork unitOfWork,
            ILogger<UserService> logger,
            IRequestContext requestContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _requestContext = requestContext;
        }

        /// <summary>
        /// Gets the current user based on authentication token or creates a new user if not found
        /// </summary>
        /// <param name="adminGroupId">The ID of the admin group to check membership against</param>
        /// <returns>The current user object</returns>
        public async Task<User> GetOrCreateCurrentUserAsync(string adminGroupId)
        {
            try
            {
                // Get user ID from claims - try objectidentifier claim first (common for Microsoft Entra ID)
                var userId = _requestContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

                // If objectIdentifier not found, try standard nameidentifier claim
                if (string.IsNullOrEmpty(userId))
                {
                    userId = _requestContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                // Ensure user ID is found in the claims
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    throw new UnauthorizedAccessException("User identity not found in token");
                }

                // Delegate to the shared method for getting or creating the user
                return await GetOrCreateUserByIdAsync(userId, adminGroupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreateCurrentUserAsync");
                throw new Exception($"Error retrieving current user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a user by ID or creates a new user if not found using claims information
        /// </summary>
        /// <param name="userId">The ID of the user to find or create</param>
        /// <param name="adminGroupId">The ID of the admin group to check membership against</param>
        /// <returns>The found or newly created user</returns>
        public async Task<User> GetOrCreateUserByIdAsync(string userId, string adminGroupId)
        {
            try
            {
                _logger.LogInformation($"Getting user with ID: {userId}");

                // Find user in database by ID
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                // If user not found, create new user based on authentication information
                if (user == null)
                {
                    _logger.LogInformation($"User {userId} not found, creating new user");

                    // Get user information from claims - try multiple claim types for maximum compatibility
                    var name = _requestContext.User.FindFirstValue("name") ??
                              _requestContext.User.FindFirstValue(ClaimTypes.Name) ??
                              "Unknown User";

                    var email = _requestContext.User.FindFirstValue("preferred_username") ??
                              _requestContext.User.FindFirstValue(ClaimTypes.Email) ??
                              _requestContext.User.FindFirstValue(ClaimTypes.Upn) ??
                              "no-email@example.com";

                    // Create new user with information extracted from claims
                    user = new User
                    {
                        Id = userId,
                        Username = name,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow
                    };

                    // Check if user is in admin group and set admin flag accordingly
                    var isAdmin = IsUserInGroup(adminGroupId);
                    user.IsAdmin = isAdmin;

                    // Save the new user to the database
                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation($"Created new user {user.Username} with ID {user.Id}");
                }
                else
                {
                    // Update login time for existing user
                    await _unitOfWork.Users.UpdateLastLoginAsync(userId);

                    // Update user's admin role if group membership has changed
                    var isAdmin = IsUserInGroup(adminGroupId);

                    if (user.IsAdmin != isAdmin)
                    {
                        await _unitOfWork.Users.UpdateAdminStatusAsync(userId, isAdmin);
                    }

                    _logger.LogInformation($"Updated login time for user {user.Username}");
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetOrCreateUserByIdAsync for user ID: {userId}");
                throw;
            }
        }

        /// <summary>
        /// Checks if the current user is in a specific Microsoft Entra ID group
        /// </summary>
        /// <param name="groupId">The ID of the group to check</param>
        /// <returns>True if the user is in the group, false otherwise</returns>
        private bool IsUserInGroup(string groupId)
        {
            // If group ID is empty, return false
            if (string.IsNullOrEmpty(groupId))
                return false;

            try
            {
                // Check "groups" claim - used by Microsoft Entra ID
                var groups = _requestContext.User.FindAll("groups")
                    .Select(c => c.Value)
                    .ToList();

                // If groups claim is missing, try traditional role claim
                if (groups.Count == 0)
                {
                    groups = _requestContext.User.FindAll(ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();
                }

                // Return true if the user has the admin group ID in their claims
                return groups.Contains(groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user is in group {groupId}");
                return false;
            }
        }

        /// <summary>
        /// Gets all users in the system
        /// </summary>
        /// <returns>Collection of all users</returns>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        /// <summary>
        /// Gets a specific user by ID
        /// </summary>
        /// <param name="id">The ID of the user to retrieve</param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        /// <summary>
        /// Creates a new user with the provided information
        /// </summary>
        /// <param name="request">User creation request containing user details</param>
        /// <returns>The newly created user</returns>
        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            // Check if user with same email already exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists");
            }

            // Create new user with generated ID
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            // Save the user to the database
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return user;
        }

        /// <summary>
        /// Updates an existing user with new information
        /// </summary>
        /// <param name="id">The ID of the user to update</param>
        /// <param name="request">User update request containing new user details</param>
        /// <returns>The updated user if found, null otherwise</returns>
        public async Task<User?> UpdateUserAsync(string id, CreateUserRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            // Check if email is already taken by another user
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (existingUser != null && existingUser.Id != id)
            {
                throw new InvalidOperationException($"Email {request.Email} is already in use by another user");
            }

            // Update user properties
            user.Username = request.Username;
            user.Email = request.Email;

            // Save changes to the database
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return user;
        }

        /// <summary>
        /// Deletes a user from the system
        /// </summary>
        /// <param name="id">The ID of the user to delete</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task DeleteUserAsync(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException($"User not found with ID {id}");
            }

            // Delete the user from the database
            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.CommitAsync();
        }
    }
}
