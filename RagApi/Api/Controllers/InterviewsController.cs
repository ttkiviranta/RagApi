using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterviewsController : BaseController
    {
        private readonly IInterviewService _interviewService;

        // Add IMapper and IRequestContext to constructor and call base constructor
        public InterviewsController(
            IInterviewService interviewService,
            IMapper mapper,
            IRequestContext requestContext)
            : base(mapper, requestContext)
        {
            _interviewService = interviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var interviews = await _interviewService.GetAllAsync();
                // Use BaseController's Success method for unified response
                return Success(interviews);
            }
            catch (Exception ex)
            {
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

                return Success(interview);
            }
            catch (Exception ex)
            {
                return Error($"Error getting interview: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InterviewCreateDto dto)
        {
            try
            {
                var interviewerId = GetCurrentUserId();
                var interview = await _interviewService.CreateAsync(dto, interviewerId);

                // Use BaseController's Created method for unified response
                return Created(interview, nameof(GetById), new { id = interview.Id });
            }
            catch (KeyNotFoundException)
            {
                return BadRequestError("Application not found");
            }
            catch (Exception ex)
            {
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

                return Success(interview);
            }
            catch (Exception ex)
            {
                return Error($"Error updating interview: {ex.Message}");
            }
        }

        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetByApplication(string applicationId)
        {
            try
            {
                var interviews = await _interviewService.GetByApplicationAsync(applicationId);
                return Success(interviews);
            }
            catch (Exception ex)
            {
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
                return Error($"Error generating interview questions: {ex.Message}");
            }
        }

        // Helper for getting current user id
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}

