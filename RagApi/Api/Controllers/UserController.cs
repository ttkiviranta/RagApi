using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagApi.Api.Models;
using RagApi.Auth;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models;

namespace RagApi.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly AzureAdOptions _azureAdOptions;
        private readonly UserMapper _userMapper;

        public UserController(
            IRequestContext requestContext,
            IUserService userService,
            IOptions<AzureAdOptions> azureAdOptions,
            ILogger<UserController> logger,
            UserMapper userMapper)
            : base(requestContext)
        {
            _userService = userService;
            _logger = logger;
            _azureAdOptions = azureAdOptions.Value;
            _userMapper = userMapper;
        }

        /// <summary>
        /// Get the current logged-in user based on token, create if not exists
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _userService.GetOrCreateCurrentUserAsync(_azureAdOptions.Groups?.Admin);
                var userDto = _userMapper.MapToDto(user);
                return Success(userDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Error(ex.Message, 401);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return Error($"Error retrieving current user: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var userDtos = users.Select(u => _userMapper.MapToDto(u)).ToList();
                return Success(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return Error($"Error retrieving users: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFoundError($"User not found with ID {id}");
                }

                var userDto = _userMapper.MapToDto(user);
                return Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                return Error($"Error retrieving user: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestError(ModelState.ToString());
            }

            try
            {
                var user = await _userService.CreateUserAsync(request);
                var userDto = _userMapper.MapToDto(user);
                return Created(userDto, nameof(GetUser), new { id = user.Id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with username {Username}", request?.Username);
                return Error($"Error creating user: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestError(ModelState.ToString());
            }

            try
            {
                var user = await _userService.UpdateUserAsync(id, request);

                if (user == null)
                {
                    return NotFoundError($"User not found with ID {id}");
                }

                var userDto = _userMapper.MapToDto(user);
                return Success(userDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {UserId}", id);
                return Error($"Error updating user: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return Success("User deleted successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFoundError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                return Error($"Error deleting user: {ex.Message}");
            }
        }
    }
}