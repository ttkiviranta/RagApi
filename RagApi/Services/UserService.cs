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

        public UserService(
            IUnitOfWork unitOfWork,
            ILogger<UserService> logger,
            IRequestContext requestContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _requestContext = requestContext;
        }

        // UserService.cs - päivitetty metodi
        public async Task<User> GetOrCreateCurrentUserAsync(string adminGroupId)
        {
            try
            {
                // Get user ID from claims
                var userId = _requestContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

                // If objectIdentifier not found, try nameidentifier claim
                if (string.IsNullOrEmpty(userId))
                {
                    userId = _requestContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                // Ensure user ID is found
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    throw new UnauthorizedAccessException("User identity not found in token");
                }

                _logger.LogInformation($"Getting user with ID: {userId}");

                // Find user in database by ID
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                // If user not found, create new user based on Azure AD information
                if (user == null)
                {
                    _logger.LogInformation($"User {userId} not found, creating new user");

                    // Get user information from claims
                    var name = _requestContext.User.FindFirstValue("name") ??
                              _requestContext.User.FindFirstValue(ClaimTypes.Name) ??
                              "Unknown User";

                    var email = _requestContext.User.FindFirstValue("preferred_username") ??
                              _requestContext.User.FindFirstValue(ClaimTypes.Email) ??
                              _requestContext.User.FindFirstValue(ClaimTypes.Upn) ??
                              "no-email@example.com";

                    // Create new user
                    user = new User
                    {
                        Id = userId,
                        Username = name,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow
                    };

                    // Check if user is in admin group
                    var isAdmin = IsUserInGroup(adminGroupId);
                    user.IsAdmin = isAdmin;

                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation($"Created new user {user.Username} with ID {user.Id}");
                }
                else
                {
                    // Update user login time
                    await _unitOfWork.Users.UpdateLastLoginAsync(userId);

                    // Update user role if needed
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
                _logger.LogError(ex, "Error in GetOrCreateCurrentUserAsync");
                throw new Exception($"Error retrieving current user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if the current user is in a specific Azure AD group
        /// </summary>
        private bool IsUserInGroup(string groupId)
        {
            // Jos ryhmän ID on tyhjä, palautetaan false
            if (string.IsNullOrEmpty(groupId))
                return false;

            try
            {
                // Check "groups" claim
                var groups = _requestContext.User.FindAll("groups")
                    .Select(c => c.Value)
                    .ToList();

                // If groups claim is missing, try traditional group claim
                if (groups.Count == 0)
                {
                    groups = _requestContext.User.FindAll(ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();
                }

                return groups.Contains(groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user is in group {groupId}");
                return false;
            }
        }


        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            // Check if user with same email already exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return user;
        }

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

            user.Username = request.Username;
            user.Email = request.Email;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return user;
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException($"User not found with ID {id}");
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.CommitAsync();
        }      
    }
}
