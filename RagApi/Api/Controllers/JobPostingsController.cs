using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models.Dto;
using RagApi.Helpers;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostingsController : BaseController
    {
        private readonly IJobPostingService _jobPostingService;
        private readonly ApplicationDbContext _dbContext;

        // Add IMapper and IRequestContext to constructor and call base constructor
        public JobPostingsController(
            IJobPostingService jobPostingService,
            ApplicationDbContext dbContext,
            IMapper mapper,
            IRequestContext requestContext)
            : base(mapper, requestContext)
        {
            _jobPostingService = jobPostingService;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var jobPostings = await _jobPostingService.GetAllAsync();
                // Use BaseController's Success method for unified response
                return Success(jobPostings);
            }
            catch (Exception ex)
            {
                return Error($"Error getting job postings: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var jobPosting = await _jobPostingService.GetByIdAsync(id);
                if (jobPosting == null)
                    return NotFoundError("Job posting not found");

                return Success(jobPosting);
            }
            catch (Exception ex)
            {
                return Error($"Error getting job posting: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobPostingCreateDto dto)
        {
            try
            {
                var userId = HttpContextHelper.GetUserIdFromRequest(HttpContext);

                // Pass userId (can be null) to service
                var jobPosting = await _jobPostingService.CreateAsync(dto, userId);

                // Use BaseController's Created method for unified response
                return Created(jobPosting, nameof(GetById), new { id = jobPosting.Id });
            }
            catch (Exception ex)
            {
                return Error($"Error creating job posting: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] JobPostingUpdateDto dto)
        {
            try
            {
                var jobPosting = await _jobPostingService.UpdateAsync(id, dto);
                if (jobPosting == null)
                    return NotFoundError("Job posting not found");

                return Success(jobPosting);
            }
            catch (Exception ex)
            {
                return Error($"Error updating job posting: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _jobPostingService.DeleteAsync(id);
                return Success(new { message = "Job posting deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Job posting not found");
            }
            catch (Exception ex)
            {
                return Error($"Error deleting job posting: {ex.Message}");
            }
        }

        [HttpPost("{id}/document")]
        public async Task<IActionResult> UploadDocument(string id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequestError("No file was uploaded");

                var userId = HttpContextHelper.GetUserIdFromRequest(HttpContext);

                // Pass userId (can be null) to service
                var documentId = await _jobPostingService.UploadDocumentAsync(id, file, userId);

                return Success(new { documentId });
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Job posting not found");
            }
            catch (Exception ex)
            {
                return Error($"Error uploading document: {ex.Message}");
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var jobPostings = await _jobPostingService.GetActiveAsync();
                return Success(jobPostings);
            }
            catch (Exception ex)
            {
                return Error($"Error getting active job postings: {ex.Message}");
            }
        }
    }
}

