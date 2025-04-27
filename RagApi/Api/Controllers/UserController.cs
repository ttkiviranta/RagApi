using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RagApi.Api.Models;
using RagApi.Data;
using RagApi.Models;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Lisätty, jotta vain kirjautuneet käyttäjät pääsevät käsiksi API:in
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext dbContext, ILogger<UserController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Get the current logged-in user based on token, create if not exists
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Hae käyttäjän ID Azure AD:n objectIdentifier-claimista
                var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

                // Jos objectIdentifier ei löydy, kokeile myös nameidentifier-claimia
                if (string.IsNullOrEmpty(userId))
                {
                    userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                // Varmista että käyttäjän ID on löytynyt
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized("User identity not found in token");
                }

                _logger.LogInformation($"Getting user with ID: {userId}");

                // Etsi käyttäjä tietokannasta ID:n perusteella
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                // Jos käyttäjää ei löydy, luo uusi käyttäjä Azure AD -tietojen perusteella
                if (user == null)
                {
                    _logger.LogInformation($"User {userId} not found, creating new user");

                    // Hae käyttäjän tiedot claimsista
                    var name = User.FindFirstValue("name") ?? User.FindFirstValue(ClaimTypes.Name) ?? "Unknown User";
                    var email = User.FindFirstValue("preferred_username") ??
                              User.FindFirstValue(ClaimTypes.Email) ??
                              User.FindFirstValue(ClaimTypes.Upn) ??
                              "no-email@example.com";

                    // Luo uusi käyttäjä
                    user = new User
                    {
                        Id = userId,
                        Username = name,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow
                    };

                    // Tarkista onko käyttäjä admin-ryhmässä
                    var isAdmin = IsUserInGroup("a6a1e23d-6f7f-48fb-b649-f9d88b9acb27"); // UserAdmin ryhmän ID
                    user.IsAdmin = isAdmin;

                    _dbContext.Users.Add(user);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Created new user {user.Username} with ID {user.Id}");
                }
                else
                {
                    // Päivitä käyttäjän kirjautumisaika
                    user.LastLogin = DateTime.UtcNow;

                    // Päivitä käyttäjän rooli jos tarpeen
                    var isAdmin = IsUserInGroup("a6a1e23d-6f7f-48fb-b649-f9d88b9acb27"); // UserAdmin ryhmän ID
                    if (user.IsAdmin != isAdmin)
                    {
                        user.IsAdmin = isAdmin;
                        user.LastRoleSync = DateTime.UtcNow;
                    }

                    // Tallenna muutokset
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Updated login time for user {user.Username}");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCurrentUser");
                return StatusCode(500, $"Error retrieving current user: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if the current user is in a specific Azure AD group
        /// </summary>
        private bool IsUserInGroup(string groupId)
        {
            try
            {
                // Tarkista "groups" claim
                var groups = User.FindAll("groups")
                    .Select(c => c.Value)
                    .ToList();

                // Jos groups claim puuttuu, kokeile myös perinteistä group claimia
                if (groups.Count == 0)
                {
                    groups = User.FindAll(ClaimTypes.Role)
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

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _dbContext.Users
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving users: {ex.Message}");
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
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound($"User not found with ID {id}");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving user: {ex.Message}");
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
                return BadRequest(ModelState);
            }

            try
            {
                // Check if user with same email already exists
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    return BadRequest($"User with email {request.Email} already exists");
                }

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
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
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _dbContext.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound($"User not found with ID {id}");
                }

                // Check if email is already taken by another user
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != id);

                if (existingUser != null)
                {
                    return BadRequest($"Email {request.Email} is already in use by another user");
                }

                user.Username = request.Username;
                user.Email = request.Email;

                await _dbContext.SaveChangesAsync();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating user: {ex.Message}");
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
                var user = await _dbContext.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound($"User not found with ID {id}");
                }

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting user: {ex.Message}");
            }
        }
    }
}