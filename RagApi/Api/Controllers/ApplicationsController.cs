using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
   // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Get all applications
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var applications = await _applicationService.GetAllAsync();
                return Ok(new { error = false, data = applications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting applications: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get an application by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var application = await _applicationService.GetByIdAsync(id);
                if (application == null)
                    return NotFound(new { error = true, message = "Application not found" });

                return Ok(new { error = false, data = application });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting application: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create a new application
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ApplicationCreateDto dto)
        {
            try
            {
                var application = await _applicationService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetById), new { id = application.Id },
                    new { error = false, data = application });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error creating application: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update an application's status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] ApplicationStatusUpdateDto dto)
        {
            try
            {
                var application = await _applicationService.UpdateStatusAsync(id, dto);
                if (application == null)
                    return NotFound(new { error = true, message = "Application not found" });

                return Ok(new { error = false, data = application });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error updating application status: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all applications for a job posting
        /// </summary>
        [HttpGet("job/{jobPostingId}")]
        public async Task<IActionResult> GetByJobPosting(string jobPostingId)
        {
            try
            {
                var applications = await _applicationService.GetByJobPostingAsync(jobPostingId);
                return Ok(new { error = false, data = applications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting applications for job posting: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all applications for a candidate
        /// </summary>
        [HttpGet("candidate/{candidateId}")]
        public async Task<IActionResult> GetByCandidate(string candidateId)
        {
            try
            {
                var applications = await _applicationService.GetByCandidateAsync(candidateId);
                return Ok(new { error = false, data = applications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting applications for candidate: {ex.Message}" });
            }
        }

        /// <summary>
        /// Generate a match score for an application
        /// </summary>
        [HttpPost("{id}/match")]
        public async Task<IActionResult> GenerateMatchScore(string id)
        {
            try
            {
                var matchResult = await _applicationService.GenerateMatchScoreAsync(id);
                return Ok(new { error = false, data = matchResult });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Application not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error generating match score: {ex.Message}" });
            }
        }
    }
}
