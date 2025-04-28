using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidatesController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        /// <summary>
        /// Get all candidates
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var candidates = await _candidateService.GetAllAsync();
                return Ok(new { error = false, data = candidates });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting candidates: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get a candidate by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var candidate = await _candidateService.GetByIdAsync(id);
                if (candidate == null)
                    return NotFound(new { error = true, message = "Candidate not found" });

                return Ok(new { error = false, data = candidate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting candidate: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create a new candidate
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CandidateCreateDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var candidate = await _candidateService.CreateAsync(dto, userId);

                return CreatedAtAction(nameof(GetById), new { id = candidate.Id },
                    new { error = false, data = candidate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error creating candidate: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update an existing candidate
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CandidateUpdateDto dto)
        {
            try
            {
                var candidate = await _candidateService.UpdateAsync(id, dto);
                if (candidate == null)
                    return NotFound(new { error = true, message = "Candidate not found" });

                return Ok(new { error = false, data = candidate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error updating candidate: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete a candidate
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _candidateService.DeleteAsync(id);
                return Ok(new { error = false, message = "Candidate deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Candidate not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error deleting candidate: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload a resume for a candidate
        /// </summary>
        [HttpPost("{id}/resume")]
        public async Task<IActionResult> UploadResume(string id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = true, message = "No file was uploaded" });

                var userId = GetCurrentUserId();
                var documentId = await _candidateService.UploadResumeAsync(id, file, userId);

                return Ok(new { error = false, data = new { documentId } });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Candidate not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error uploading resume: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload a cover letter for a candidate
        /// </summary>
        [HttpPost("{id}/cover-letter")]
        public async Task<IActionResult> UploadCoverLetter(string id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = true, message = "No file was uploaded" });

                var userId = GetCurrentUserId();
                var documentId = await _candidateService.UploadCoverLetterAsync(id, file, userId);

                return Ok(new { error = false, data = new { documentId } });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Candidate not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error uploading cover letter: {ex.Message}" });
            }
        }

        private string GetCurrentUserId()
        {
            // Get the logged-in user's ID
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
