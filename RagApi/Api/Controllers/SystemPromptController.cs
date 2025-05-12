using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemPromptController : ControllerBase
    {
        private readonly ISystemPromptService _systemPromptService;

        public SystemPromptController(ISystemPromptService systemPromptService)
        {
            _systemPromptService = systemPromptService;
        }

        /// <summary>
        /// Gets all system prompts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSystemPrompts()
        {
            try
            {
                var systemPrompts = await _systemPromptService.GetSystemPromptsAsync();
                return Ok(systemPrompts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving system prompts: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a system prompt by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSystemPrompt(string id)
        {
            try
            {
                var systemPrompt = await _systemPromptService.GetSystemPromptAsync(id);
                if (systemPrompt == null)
                {
                    return NotFound($"System prompt not found with ID {id}");
                }

                return Ok(systemPrompt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving system prompt: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the default system prompt
        /// </summary>
        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultSystemPrompt()
        {
            try
            {
                var systemPrompt = await _systemPromptService.GetDefaultSystemPromptAsync();
                if (systemPrompt == null)
                {
                    return NotFound("No default system prompt has been set");
                }

                return Ok(systemPrompt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving default system prompt: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new system prompt
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSystemPrompt([FromBody] CreateSystemPromptRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var systemPrompt = await _systemPromptService.CreateSystemPromptAsync(
                    request.Name,
                    request.Description,
                    request.PromptText,
                    request.IsDefault);

                return CreatedAtAction(nameof(GetSystemPrompt), new { id = systemPrompt.Id }, systemPrompt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating system prompt: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing system prompt
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSystemPrompt(string id, [FromBody] UpdateSystemPromptRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var systemPrompt = await _systemPromptService.UpdateSystemPromptAsync(
                    id,
                    request.Name,
                    request.Description,
                    request.PromptText,
                    request.IsDefault);

                return Ok(systemPrompt);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating system prompt: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a system prompt
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystemPrompt(string id)
        {
            try
            {
                var result = await _systemPromptService.DeleteSystemPromptAsync(id);
                if (!result)
                {
                    return NotFound($"System prompt not found with ID {id}");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting system prompt: {ex.Message}");
            }
        }

        /// <summary>
        /// Assigns a system prompt to a user
        /// </summary>
        [HttpPost("user/{userId}/assign/{systemPromptId}")]
        public async Task<IActionResult> AssignSystemPromptToUser(string userId, string systemPromptId, [FromBody] AssignSystemPromptRequest request)
        {
            try
            {
                var result = await _systemPromptService.AssignSystemPromptToUserAsync(
                    userId,
                    systemPromptId,
                    request.IsDefault);

                if (!result)
                {
                    return NotFound("User or system prompt not found");
                }

                return Ok(new { Message = "System prompt assigned to user successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error assigning system prompt to user: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a user's system prompt
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserSystemPrompt(string userId)
        {
            try
            {
                var systemPrompt = await _systemPromptService.GetUserSystemPromptAsync(userId);
                if (systemPrompt == null)
                {
                    return NotFound($"No system prompt found for user {userId}");
                }

                return Ok(systemPrompt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving user's system prompt: {ex.Message}");
            }
        }
    }

    public class CreateSystemPromptRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string PromptText { get; set; }

        public bool IsDefault { get; set; }
    }

    public class UpdateSystemPromptRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string PromptText { get; set; }

        public bool IsDefault { get; set; }
    }

    public class AssignSystemPromptRequest
    {
        public bool IsDefault { get; set; } = true;
    }
}
