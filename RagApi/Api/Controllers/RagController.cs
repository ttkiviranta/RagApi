using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagApi.Api.Models;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly IRagService _ragService;

        public RagController(IRagService ragService)
        {
            _ragService = ragService;
        }

        /// <summary>
        /// Upload and process a document (PDF, Word, etc.)
        /// </summary>
        [HttpPost("documents")]
        public async Task<IActionResult> UploadDocument(IFormFile file, [FromForm] UploadDocumentRequest request)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // Currently only supporting PDF files
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF files are supported at this time.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                string documentId = await _ragService.ProcessPdfAsync(stream, file.FileName);

                return Ok(new UploadDocumentResponse
                {
                    DocumentId = documentId,
                    Message = "Document processed successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing document: {ex.Message}");
            }
        }

        /// <summary>
        /// Make a single query without conversation context
        /// </summary>
        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] QueryRequest request)
        {
            if (string.IsNullOrEmpty(request?.Query))
            {
                return BadRequest("Query cannot be empty.");
            }

            try
            {
                var userId = GetUserIdFromRequest();
                var response = await _ragService.QueryAsync(request.Query, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing query: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new conversation
        /// </summary>
        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            if (string.IsNullOrEmpty(request?.Title))
            {
                return BadRequest("Conversation title cannot be empty.");
            }

            try
            {
                var userId = GetUserIdFromRequest();
                var conversation = await _ragService.CreateConversationAsync(request.Title, userId);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating conversation: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all conversations
        /// </summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = GetUserIdFromRequest();
                var conversations = await _ragService.GetConversationsAsync(userId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving conversations: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a single conversation
        /// </summary>
        [HttpGet("conversations/{conversationId}")]
        public async Task<IActionResult> GetConversation(string conversationId)
        {
            try
            {
                var conversation = await _ragService.GetConversationAsync(conversationId);
                if (conversation == null)
                {
                    return NotFound($"Conversation not found with ID {conversationId}");
                }

                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving conversation: {ex.Message}");
            }
        }

        /// <summary>
        /// Make a query in a conversation (using conversation history)
        /// </summary>
        [HttpPost("conversations/{conversationId}/query")]
        public async Task<IActionResult> QueryInConversation(string conversationId, [FromBody] QueryRequest request)
        {
            if (string.IsNullOrEmpty(request?.Query))
            {
                return BadRequest("Query cannot be empty.");
            }

            try
            {
                var userId = GetUserIdFromRequest();
                var response = await _ragService.QueryWithConversationAsync(conversationId, request.Query, userId);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing query in conversation: {ex.Message}");
            }
        }

        /// <summary>
        /// Match job postings with candidate resumes
        /// </summary>
        [HttpPost("job-matching")]
        public async Task<IActionResult> MatchJobCandidates([FromBody] JobMatchingRequest request)
        {
            if (string.IsNullOrEmpty(request?.JobPosting))
            {
                return BadRequest("Job posting cannot be empty.");
            }

            try
            {
                // Use the HR job matching specialized prompt
                var userId = GetUserIdFromRequest();
                var response = await _ragService.QueryAsync(
                    $"Analyze the following job posting and find the best matching candidates:\n\n{request.JobPosting}",
                    userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing job matching request: {ex.Message}");
            }
        }

        // Helper method to get userId from request
        // In a real implementation, this would come from auth middleware
        private string GetUserIdFromRequest()
        {
            // For now, just return null to use default system prompt
            // In a real implementation, this would extract the user ID from the auth token
            return HttpContext.Request.Headers.TryGetValue("X-User-Id", out var userId)
                ? userId.ToString()
                : null;
        }
    }
}
