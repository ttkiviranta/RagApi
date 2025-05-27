using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RagApi.Api.Auth;
using RagApi.Api.Controllers; // Add this reference
using RagApi.Interfaces; // Add this for IRequestContext
using AutoMapper; // Add this for IMapper
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

        /// <summary>
        /// Initializes a new instance of the AuthController
        /// </summary>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="requestContext">Request context</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="authService">Authentication service instance</param>
        public AuthController(
            IMapper mapper,
            IRequestContext requestContext,
            ILogger<AuthController> logger,
            EntraAuthService authService)
            : base(mapper, requestContext) // Pass required parameters to base constructor
        {
            _logger = logger;
            _authService = authService;
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

                return Success(new
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.Username, // Changed to Username based on User class signature
                    IsActive = true // User class doesn't have IsActive property, so using a default value
                });
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
                UserId = RequestContext.GetCurrentUserId() // Use RequestContext instead of HttpContext extension method
            });
        }
    }
}