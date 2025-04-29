using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
  //  [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InterviewsController : ControllerBase
    {
        private readonly IInterviewService _interviewService;

        public InterviewsController(IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        /// <summary>
        /// Get all interviews
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var interviews = await _interviewService.GetAllAsync();
                return Ok(new { error = false, data = interviews });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting interviews: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get an interview by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var interview = await _interviewService.GetByIdAsync(id);
                if (interview == null)
                    return NotFound(new { error = true, message = "Interview not found" });

                return Ok(new { error = false, data = interview });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting interview: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create a new interview
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InterviewCreateDto dto)
        {
            try
            {
                var interviewerId = GetCurrentUserId();
                var interview = await _interviewService.CreateAsync(dto, interviewerId);

                return CreatedAtAction(nameof(GetById), new { id = interview.Id },
                    new { error = false, data = interview });
            }
            catch (KeyNotFoundException)
            {
                return BadRequest(new { error = true, message = "Application not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error creating interview: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update an existing interview
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] InterviewUpdateDto dto)
        {
            try
            {
                var interview = await _interviewService.UpdateAsync(id, dto);
                if (interview == null)
                    return NotFound(new { error = true, message = "Interview not found" });

                return Ok(new { error = false, data = interview });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error updating interview: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all interviews for an application
        /// </summary>
        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetByApplication(string applicationId)
        {
            try
            {
                var interviews = await _interviewService.GetByApplicationAsync(applicationId);
                return Ok(new { error = false, data = interviews });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting interviews for application: {ex.Message}" });
            }
        }

        /// <summary>
        /// Generate interview questions
        /// </summary>
        [HttpPost("{id}/questions")]
        public async Task<IActionResult> GenerateInterviewQuestions(string id)
        {
            try
            {
                var questions = await _interviewService.GenerateInterviewQuestionsAsync(id);
                return Ok(new { error = false, data = questions });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Interview not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error generating interview questions: {ex.Message}" });
            }
        }

        private string GetCurrentUserId()
        {
            // Get the logged-in user's ID
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
