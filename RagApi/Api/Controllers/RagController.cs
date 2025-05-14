using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagApi.Api.Models;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : BaseController
    {
        private readonly IRagService _ragService;

        // Add IMapper and IRequestContext to constructor and call base constructor
        public RagController(
            IRagService ragService,
            IMapper mapper,
            IRequestContext requestContext)
            : base(mapper, requestContext)
        {
            _ragService = ragService;
        }

        [HttpPost("documents")]
        public async Task<IActionResult> UploadDocument(IFormFile file, [FromForm] UploadDocumentRequest request)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequestError("No file was uploaded.");
            }

            // Only PDF files are supported
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequestError("Only PDF files are supported at this time.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                string documentId = await _ragService.ProcessPdfAsync(stream, file.FileName);

                return Success(new UploadDocumentResponse
                {
                    DocumentId = documentId,
                    Message = "Document processed successfully."
                });
            }
            catch (Exception ex)
            {
                return Error($"Error processing document: {ex.Message}");
            }
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] QueryRequest request)
        {
            if (string.IsNullOrEmpty(request?.Query))
            {
                return BadRequestError("Query cannot be empty.");
            }

            try
            {
                var userId = GetUserIdFromClaims();
                var response = await _ragService.QueryAsync(request.Query, userId);
                return Success(response);
            }
            catch (Exception ex)
            {
                return Error($"Error processing query: {ex.Message}");
            }
        }

        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            if (string.IsNullOrEmpty(request?.Title))
            {
                return BadRequestError("Conversation title cannot be empty.");
            }

            try
            {
                var userId = GetUserIdFromClaims();
                var conversation = await _ragService.CreateConversationAsync(request.Title, userId);
                return Success(conversation);
            }
            catch (Exception ex)
            {
                return Error($"Error creating conversation: {ex.Message}");
            }
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var conversations = await _ragService.GetConversationsAsync(userId);
                return Success(conversations);
            }
            catch (Exception ex)
            {
                return Error($"Error retrieving conversations: {ex.Message}");
            }
        }

        [HttpGet("conversations/{conversationId}")]
        public async Task<IActionResult> GetConversation(string conversationId)
        {
            try
            {
                var conversation = await _ragService.GetConversationAsync(conversationId);
                if (conversation == null)
                {
                    return NotFoundError($"Conversation not found with ID {conversationId}");
                }

                return Success(conversation);
            }
            catch (Exception ex)
            {
                return Error($"Error retrieving conversation: {ex.Message}");
            }
        }

        [HttpPost("conversations/{conversationId}/query")]
        public async Task<IActionResult> QueryInConversation(string conversationId, [FromBody] QueryRequest request)
        {
            if (string.IsNullOrEmpty(request?.Query))
            {
                return BadRequestError("Query cannot be empty.");
            }

            try
            {
                var userId = GetUserIdFromClaims();
                var response = await _ragService.QueryWithConversationAsync(conversationId, request.Query, userId);
                return Success(response);
            }
            catch (ArgumentException ex)
            {
                return NotFoundError(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Error processing query in conversation: {ex.Message}");
            }
        }

        [HttpPost("job-matching")]
        public async Task<IActionResult> MatchJobCandidates([FromBody] JobMatchingRequest request)
        {
            if (string.IsNullOrEmpty(request?.JobPosting))
            {
                return BadRequestError("Job posting cannot be empty.");
            }

            try
            {
                var userId = GetUserIdFromClaims();
                var response = await _ragService.QueryAsync(
                    $"Analyze the following job posting and find the best matching candidates:\n\n{request.JobPosting}",
                    userId);

                return Success(response);
            }
            catch (Exception ex)
            {
                return Error($"Error processing job matching request: {ex.Message}");
            }
        }

        // Helper for getting user id from claims (SSO)
        private string? GetUserIdFromClaims()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
