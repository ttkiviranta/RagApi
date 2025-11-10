using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostingsController : BaseController
    {
        private readonly IJobPostingService _jobPostingService;
        private readonly ApplicationDbContext _dbContext;
        private readonly JobPostingMapper _jobPostingMapper;
        private readonly ILogger<JobPostingsController> _logger;

        public JobPostingsController(
            IJobPostingService jobPostingService,
            ApplicationDbContext dbContext,
            IRequestContext requestContext,
            JobPostingMapper jobPostingMapper,
            ILogger<JobPostingsController> logger)
            : base(requestContext)
        {
            _jobPostingService = jobPostingService;
            _dbContext = dbContext;
            _jobPostingMapper = jobPostingMapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var jobPostings = await _jobPostingService.GetAllAsync();
                var jobPostingDtos = jobPostings.Select(jp => _jobPostingMapper.MapToDto(jp)).ToList();
                return Success(jobPostingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all job postings");
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

                var jobPostingDto = _jobPostingMapper.MapToDto(jobPosting);
                return Success(jobPostingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job posting with ID {JobPostingId}", id);
                return Error($"Error getting job posting: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobPostingCreateDto dto)
        {
            try
            {
                var userId = RequestContext.GetCurrentUserId();
                var jobPosting = await _jobPostingService.CreateAsync(dto, userId);
                var jobPostingDto = _jobPostingMapper.MapToDto(jobPosting);

                return Created(jobPostingDto, nameof(GetById), new { id = jobPosting.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job posting");
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

                var jobPostingDto = _jobPostingMapper.MapToDto(jobPosting);
                return Success(jobPostingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job posting with ID {JobPostingId}", id);
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
                _logger.LogError(ex, "Error deleting job posting with ID {JobPostingId}", id);
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

                var userId = RequestContext.GetCurrentUserId();
                var documentId = await _jobPostingService.UploadDocumentAsync(id, file, userId);

                return Success(new { documentId });
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Job posting not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for job posting {JobPostingId}", id);
                return Error($"Error uploading document: {ex.Message}");
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var jobPostings = await _jobPostingService.GetActiveAsync();
                var jobPostingDtos = jobPostings.Select(jp => _jobPostingMapper.MapToDto(jp)).ToList();
                return Success(jobPostingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active job postings");
                return Error($"Error getting active job postings: {ex.Message}");
            }
        }
    }
}