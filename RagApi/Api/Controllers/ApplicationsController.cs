using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : BaseController
    {
        private readonly IApplicationService _applicationService;
        private readonly ApplicationMapper _applicationMapper;

        public ApplicationsController(
            IRequestContext requestContext,
            IApplicationService applicationService,
            ApplicationMapper applicationMapper)
            : base(requestContext)
        {
            _applicationService = applicationService;
            _applicationMapper = applicationMapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var applications = await _applicationService.GetAllAsync();
                var applicationDtos = applications.Select(app => _applicationMapper.MapToDto(app)).ToList();
                return Success(applicationDtos);
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

                var applicationDto = _applicationMapper.MapToDto(application);
                return Success(applicationDto);
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
                var applicationDto = _applicationMapper.MapToDto(application);
                return Created(applicationDto, nameof(GetById), new { id = application.Id });
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

                var applicationDto = _applicationMapper.MapToDto(application);
                return Success(applicationDto);
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
                var applicationDtos = applications.Select(app => _applicationMapper.MapToDto(app)).ToList();
                return Success(applicationDtos);
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
                var applicationDtos = applications.Select(app => _applicationMapper.MapToDto(app)).ToList();
                return Success(applicationDtos);
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