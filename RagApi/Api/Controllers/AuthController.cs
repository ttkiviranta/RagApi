using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RagApi.Api.Auth;
using RagApi.Interfaces;
using RagApi.Mapping;
using System;
using System.Threading.Tasks;

namespace RagApi.Api.Controllers
{
    /// <summary>
    /// Controller for authentication operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly ILogger<AuthController> _logger;
        private readonly EntraAuthService _authService;
        private readonly UserMapper _userMapper;

        /// <summary>
        /// Initializes a new instance of the AuthController
        /// </summary>
        /// <param name="requestContext">Request context</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="authService">Authentication service instance</param>
        /// <param name="userMapper">User mapper instance</param>
        public AuthController(
            IRequestContext requestContext,
            ILogger<AuthController> logger,
            EntraAuthService authService,
            UserMapper userMapper)
            : base(requestContext) // Päivitetty ottamaan vain requestContext
        {
            _logger = logger;
            _authService = authService;
            _userMapper = userMapper;
        }

        /// <summary>
        /// Gets current user information if authenticated
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _authService.GetOrCreateUserAsync();
                if (user == null)
                {
                    return Unauthorized();
                }

                // Käytä UserMapper-luokkaa muuntamaan käyttäjä DTO-objektiksi
                var userDto = _userMapper.MapToDto(user);
                return Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user information");
                return Error("An error occurred while retrieving user information");
            }
        }

        /// <summary>
        /// Verifies that the token is valid and returns basic info
        /// </summary>
        [HttpGet("verify")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            return Success(new
            {
                Message = "Token is valid",
                UserId = RequestContext.GetCurrentUserId()
            });
        }
    }
}