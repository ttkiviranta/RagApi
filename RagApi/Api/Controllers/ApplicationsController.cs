using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : BaseController
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(
            IApplicationService applicationService,
            IMapper mapper,
            IRequestContext requestContext)
            : base(mapper, requestContext)
        {
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var applications = await _applicationService.GetAllAsync();
                return Success(applications);
            }
            catch (Exception ex)
            {
                return Error($"Error getting applications: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var application = await _applicationService.GetByIdAsync(id);
                if (application == null)
                    return NotFoundError("Application not found");

                return Success(application);
            }
            catch (Exception ex)
            {
                return Error($"Error getting application: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ApplicationCreateDto dto)
        {
            try
            {
                var application = await _applicationService.CreateAsync(dto);
                return Created(application, nameof(GetById), new { id = application.Id });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Error creating application: {ex.Message}");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] ApplicationStatusUpdateDto dto)
        {
            try
            {
                var application = await _applicationService.UpdateStatusAsync(id, dto);
                if (application == null)
                    return NotFoundError("Application not found");

                return Success(application);
            }
            catch (Exception ex)
            {
                return Error($"Error updating application status: {ex.Message}");
            }
        }

        [HttpGet("job/{jobPostingId}")]
        public async Task<IActionResult> GetByJobPosting(string jobPostingId)
        {
            try
            {
                var applications = await _applicationService.GetByJobPostingAsync(jobPostingId);
                return Success(applications);
            }
            catch (Exception ex)
            {
                return Error($"Error getting applications for job posting: {ex.Message}");
            }
        }

        [HttpGet("candidate/{candidateId}")]
        public async Task<IActionResult> GetByCandidate(string candidateId)
        {
            try
            {
                var applications = await _applicationService.GetByCandidateAsync(candidateId);
                return Success(applications);
            }
            catch (Exception ex)
            {
                return Error($"Error getting applications for candidate: {ex.Message}");
            }
        }

        [HttpPost("{id}/match")]
        public async Task<IActionResult> GenerateMatchScore(string id)
        {
            try
            {
                var matchResult = await _applicationService.GenerateMatchScoreAsync(id);
                return Success(matchResult);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Application not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Error generating match score: {ex.Message}");
            }
        }
    }
}
