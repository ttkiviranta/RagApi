using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidatesController : BaseController
    {
        private readonly ICandidateService _candidateService;
        private readonly CandidateMapper _candidateMapper;
        private readonly ILogger<CandidatesController> _logger;

        public CandidatesController(
           IRequestContext requestContext,
           ApplicationDbContext dbContext,
           ICandidateService candidateService,
           CandidateMapper candidateMapper,
           ILogger<CandidatesController> logger)
           : base(requestContext)
        {
            _candidateService = candidateService;
            _candidateMapper = candidateMapper;
            _logger = logger;
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
                var candidateDtos = candidates.Select(c => _candidateMapper.MapToDto(c)).ToList();
                return Success(candidateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all candidates");
                return Error($"Error getting candidates: {ex.Message}");
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
                    return NotFoundError("Candidate not found");

                var candidateDto = _candidateMapper.MapToDto(candidate);
                return Success(candidateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidate with ID {CandidateId}", id);
                return Error($"Error getting candidate: {ex.Message}");
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
                // Tarkista vain pakolliset kentät
                var validationErrors = new List<string>();
                if (string.IsNullOrWhiteSpace(dto.FirstName))
                    validationErrors.Add("First name is required");
                if (string.IsNullOrWhiteSpace(dto.LastName))
                    validationErrors.Add("Last name is required");
                if (string.IsNullOrWhiteSpace(dto.Email))
                    validationErrors.Add("Email is required");

                if (validationErrors.Any())
                    return BadRequestError($"Validation failed: {string.Join(", ", validationErrors)}");

                // Varmista, että valinnaiset kentät eivät ole null
                dto.Skills = dto.Skills ?? "";
                dto.LinkedInProfile = dto.LinkedInProfile ?? "";
                dto.PhoneNumber = dto.PhoneNumber ?? "";
                dto.Location = dto.Location ?? "";
                dto.CurrentPosition = dto.CurrentPosition ?? "";
                dto.CurrentCompany = dto.CurrentCompany ?? "";

                // Jos käyttäjä on kirjautunut, lisätään tieto siitä kuka loi kandidaatin (audit trail)
                // mutta toiminta ei vaadi tunnistettua käyttäjää
                var userId = RequestContext.GetCurrentUserId();
                var candidate = await _candidateService.CreateAsync(dto, userId);

                var candidateDto = _candidateMapper.MapToDto(candidate);
                return Created(candidateDto, nameof(GetById), new { id = candidate.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating a new candidate");
                return Error($"Error creating candidate: {ex.Message}");
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
                    return NotFoundError("Candidate not found");

                var candidateDto = _candidateMapper.MapToDto(candidate);
                return Success(candidateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate with ID {CandidateId}", id);
                return Error($"Error updating candidate: {ex.Message}");
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
                return Success(new { message = "Candidate deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Candidate not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting candidate with ID {CandidateId}", id);
                return Error($"Error deleting candidate: {ex.Message}");
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
                    return BadRequestError("No file was uploaded");

                var userId = RequestContext.GetCurrentUserId();
                var documentId = await _candidateService.UploadResumeAsync(id, file, userId);

                return Success(new { documentId });
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Candidate not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading resume for candidate {CandidateId}", id);
                return Error($"Error uploading resume: {ex.Message}");
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
                    return BadRequestError("No file was uploaded");

                var userId = RequestContext.GetCurrentUserId();
                var documentId = await _candidateService.UploadCoverLetterAsync(id, file, userId);

                return Success(new { documentId });
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Candidate not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading cover letter for candidate {CandidateId}", id);
                return Error($"Error uploading cover letter: {ex.Message}");
            }
        }
    }
}