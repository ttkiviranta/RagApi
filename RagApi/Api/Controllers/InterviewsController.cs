using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterviewsController : BaseController
    {
        private readonly IInterviewService _interviewService;
        private readonly InterviewMapper _interviewMapper;
        private readonly ILogger<InterviewsController> _logger;

        public InterviewsController(
            IInterviewService interviewService,
            InterviewMapper interviewMapper,
            IRequestContext requestContext,
            ILogger<InterviewsController> logger)
            : base(requestContext)
        {
            _interviewService = interviewService;
            _interviewMapper = interviewMapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var interviews = await _interviewService.GetAllAsync();
                var interviewDtos = interviews.Select(i => _interviewMapper.MapToDto(i)).ToList();
                return Success(interviewDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all interviews");
                return Error($"Error getting interviews: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var interview = await _interviewService.GetByIdAsync(id);
                if (interview == null)
                    return NotFoundError("Interview not found");

                var interviewDto = _interviewMapper.MapToDto(interview);
                return Success(interviewDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interview with ID {InterviewId}", id);
                return Error($"Error getting interview: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InterviewCreateDto dto)
        {
            try
            {
                var interviewerId = RequestContext.GetCurrentUserId();
                var interview = await _interviewService.CreateAsync(dto, interviewerId);
                var interviewDto = _interviewMapper.MapToDto(interview);

                return Created(interviewDto, nameof(GetById), new { id = interview.Id });
            }
            catch (KeyNotFoundException)
            {
                return BadRequestError("Application not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating a new interview");
                return Error($"Error creating interview: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] InterviewUpdateDto dto)
        {
            try
            {
                var interview = await _interviewService.UpdateAsync(id, dto);
                if (interview == null)
                    return NotFoundError("Interview not found");

                var interviewDto = _interviewMapper.MapToDto(interview);
                return Success(interviewDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating interview with ID {InterviewId}", id);
                return Error($"Error updating interview: {ex.Message}");
            }
        }

        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetByApplication(string applicationId)
        {
            try
            {
                var interviews = await _interviewService.GetByApplicationAsync(applicationId);
                var interviewDtos = interviews.Select(i => _interviewMapper.MapToDto(i)).ToList();
                return Success(interviewDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interviews for application {ApplicationId}", applicationId);
                return Error($"Error getting interviews for application: {ex.Message}");
            }
        }

        [HttpPost("{id}/questions")]
        public async Task<IActionResult> GenerateInterviewQuestions(string id)
        {
            try
            {
                var questions = await _interviewService.GenerateInterviewQuestionsAsync(id);
                return Success(questions);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Interview not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating interview questions for interview {InterviewId}", id);
                return Error($"Error generating interview questions: {ex.Message}");
            }
        }
    }
}