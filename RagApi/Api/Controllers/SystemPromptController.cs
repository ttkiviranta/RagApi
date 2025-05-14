using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemPromptController : BaseController
    {
        private readonly ISystemPromptService _systemPromptService;

        public SystemPromptController(
            IMapper mapper,
            IRequestContext requestContext,
            ISystemPromptService systemPromptService)
            : base(mapper, requestContext)
        {
            _systemPromptService = systemPromptService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemPrompts()
        {
            try
            {
                var systemPrompts = await _systemPromptService.GetSystemPromptsAsync();
                return Success(systemPrompts);
            }
            catch (Exception ex)
            {
                return Error($"Error retrieving system prompts: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSystemPrompt(string id)
        {
            try
            {
                var systemPrompt = await _systemPromptService.GetSystemPromptAsync(id);
                if (systemPrompt == null)
                {
                    return NotFoundError($"System prompt not found with ID {id}");
                }

                return Success(systemPrompt);
            }
            catch (Exception ex)
            {
                return Error($"Error retrieving system prompt: {ex.Message}");
            }
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultSystemPrompt()
        {
            try
            {
                var systemPrompt = await _systemPromptService.GetDefaultSystemPromptAsync();
                if (systemPrompt == null)
                {
                    return NotFoundError("No default system prompt has been set");
                }

                return Success(systemPrompt);
            }
            catch (Exception ex)
            {
                return Error($"Error retrieving default system prompt: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSystemPrompt([FromBody] CreateSystemPromptRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestError(ModelState.ToString());
            }

            try
            {
                var systemPrompt = await _systemPromptService.CreateSystemPromptAsync(
                    request.Name,
                    request.Description,
                    request.PromptText,
                    request.IsDefault);

                return Created(systemPrompt, nameof(GetSystemPrompt), new { id = systemPrompt.Id });
            }
            catch (Exception ex)
            {
                return Error($"Error creating system prompt: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSystemPrompt(string id, [FromBody] UpdateSystemPromptRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestError(ModelState.ToString());
            }

            try
            {
                var systemPrompt = await _systemPromptService.UpdateSystemPromptAsync(
                    id,
                    request.Name,
                    request.Description,
                    request.PromptText,
                    request.IsDefault);

                return Success(systemPrompt);
            }
            catch (ArgumentException ex)
            {
                return NotFoundError(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Error updating system prompt: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystemPrompt(string id)
        {
            try
            {
                var result = await _systemPromptService.DeleteSystemPromptAsync(id);
                if (!result)
                {
                    return NotFoundError($"System prompt not found with ID {id}");
                }

                // NoContent returns HTTP 204 with no body
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Error deleting system prompt: {ex.Message}");
            }
        }

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
                    return NotFoundError("User or system prompt not found");
                }

                return Success(new { Message = "System prompt assigned to user successfully" });
            }
            catch (Exception ex)
            {
                return Error($"Error assigning system prompt to user: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserSystemPrompt(string userId)
        {
            try
            {
                var systemPrompt = await _systemPromptService.GetUserSystemPromptAsync(userId);
                if (systemPrompt == null)
                {
                    return NotFoundError($"No system prompt found for user {userId}");
                }

                return Success(systemPrompt);
            }
            catch (Exception ex)
            {
                return Error($"Error retrieving user's system prompt: {ex.Message}");
            }
        }
    }
}
