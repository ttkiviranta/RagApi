using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
   // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostingsController : ControllerBase
    {
        private readonly IJobPostingService _jobPostingService;

        public JobPostingsController(IJobPostingService jobPostingService)
        {
            _jobPostingService = jobPostingService;
        }

        /// <summary>
        /// Get all job postings
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var jobPostings = await _jobPostingService.GetAllAsync();
                return Ok(new { error = false, data = jobPostings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting job postings: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get a job posting by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var jobPosting = await _jobPostingService.GetByIdAsync(id);
                if (jobPosting == null)
                    return NotFound(new { error = true, message = "Job posting not found" });

                return Ok(new { error = false, data = jobPosting });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting job posting: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create a new job posting
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobPostingCreateDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var jobPosting = await _jobPostingService.CreateAsync(dto, userId);

                return CreatedAtAction(nameof(GetById), new { id = jobPosting.Id },
                    new { error = false, data = jobPosting });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error creating job posting: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update an existing job posting
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] JobPostingUpdateDto dto)
        {
            try
            {
                var jobPosting = await _jobPostingService.UpdateAsync(id, dto);
                if (jobPosting == null)
                    return NotFound(new { error = true, message = "Job posting not found" });

                return Ok(new { error = false, data = jobPosting });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error updating job posting: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete a job posting
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _jobPostingService.DeleteAsync(id);
                return Ok(new { error = false, message = "Job posting deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Job posting not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error deleting job posting: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload a document for a job posting
        /// </summary>
        [HttpPost("{id}/document")]
        public async Task<IActionResult> UploadDocument(string id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = true, message = "No file was uploaded" });

                var userId = GetCurrentUserId();
                var documentId = await _jobPostingService.UploadDocumentAsync(id, file, userId);

                return Ok(new { error = false, data = new { documentId } });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Job posting not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error uploading document: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get active job postings
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var jobPostings = await _jobPostingService.GetActiveAsync();
                return Ok(new { error = false, data = jobPostings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting active job postings: {ex.Message}" });
            }
        }

        private string GetCurrentUserId()
        {
            // Get the logged-in user's ID
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
