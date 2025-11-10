// RagController.cs - Controller for RAG (Retrieval Augmented Generation) functionality
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RagApi.Api.Models;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : BaseController
    {
        private readonly IRagService _ragService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RagController> _logger;

        /// <summary>
        /// Initializes a new instance of the RagController class
        /// </summary>
        /// <param name="ragService">Service for RAG operations</param>
        /// <param name="userService">Service for user management</param>
        /// <param name="requestContext">Context for the current request</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger for controller operations</param>
        public RagController(
            IRagService ragService,
            IUserService userService,
            IRequestContext requestContext,
            IConfiguration configuration,
            ILogger<RagController> logger)
            : base(requestContext)
        {
            _ragService = ragService;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Uploads and processes a document for RAG
        /// </summary>
        /// <param name="file">The PDF file to upload</param>
        /// <param name="request">Additional metadata for the document</param>
        /// <returns>Action result with document ID and status message</returns>
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
                _logger.LogError(ex, "Error processing document {FileName}", file.FileName);
                return Error($"Error processing document: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs a query against the RAG system
        /// </summary>
        /// <param name="request">The query request</param>
        /// <returns>Action result with the response from the RAG system</returns>
        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] QueryRequest request)
        {
            if (string.IsNullOrEmpty(request?.Query))
            {
                return BadRequestError("Query cannot be empty.");
            }

            try
            {
                var userId = await GetOrCreateUserIdAsync();
                var response = await _ragService.QueryAsync(request.Query, userId);
                return Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing query: {Query}",
                    request?.Query?.Length > 50 ? request?.Query?.Substring(0, 50) + "..." : request?.Query);
                return Error($"Error processing query: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        /// <param name="request">The conversation creation request</param>
        /// <returns>Action result with the created conversation</returns>
        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            if (string.IsNullOrEmpty(request?.Title))
            {
                return BadRequestError("Conversation title cannot be empty.");
            }

            try
            {
                // Get or create user, ensuring a user always exists
                var userId = await GetOrCreateUserIdAsync();

                // Create the conversation with the validated user ID
                var conversation = await _ragService.CreateConversationAsync(request.Title, userId);

                return Success(conversation);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Authentication error during conversation creation");
                return Unauthorized(new { error = true, message = "Authentication required to create a conversation" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation with title {Title}", request?.Title);
                return Error($"Error creating conversation: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all conversations for the current user
        /// </summary>
        /// <returns>Action result with the list of conversations</returns>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = await GetOrCreateUserIdAsync();
                var conversations = await _ragService.GetConversationsAsync(userId);
                return Success(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations");
                return Error($"Error retrieving conversations: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a specific conversation by ID
        /// </summary>
        /// <param name="conversationId">The ID of the conversation to retrieve</param>
        /// <returns>Action result with the requested conversation</returns>
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
                _logger.LogError(ex, "Error retrieving conversation with ID {ConversationId}", conversationId);
                return Error($"Error retrieving conversation: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs a query within a specific conversation context
        /// </summary>
        /// <param name="conversationId">The ID of the conversation</param>
        /// <param name="request">The query request</param>
        /// <returns>Action result with the response from the RAG system</returns>
        [HttpPost("conversations/{conversationId}/query")]
        public async Task<IActionResult> QueryInConversation(string conversationId, [FromBody] QueryRequest request)
        {
            if (string.IsNullOrEmpty(request?.Query))
            {
                return BadRequestError("Query cannot be empty.");
            }

            try
            {
                var userId = await GetOrCreateUserIdAsync();
                var response = await _ragService.QueryWithConversationAsync(conversationId, request.Query, userId);
                return Success(response);
            }
            catch (ArgumentException ex)
            {
                return NotFoundError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing query in conversation {ConversationId}", conversationId);
                return Error($"Error processing query in conversation: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs job matching analysis against candidate profiles
        /// </summary>
        /// <param name="request">The job matching request with job posting details</param>
        /// <returns>Action result with matching candidate analysis</returns>
        [HttpPost("job-matching")]
        public async Task<IActionResult> MatchJobCandidates([FromBody] JobMatchingRequest request)
        {
            if (string.IsNullOrEmpty(request?.JobPosting))
            {
                return BadRequestError("Job posting cannot be empty.");
            }

            try
            {
                var userId = await GetOrCreateUserIdAsync();
                var response = await _ragService.QueryAsync(
                    $"Analyze the following job posting and find the best matching candidates:\n\n{request.JobPosting}",
                    userId);

                return Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job matching request");
                return Error($"Error processing job matching request: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to get a valid user ID, creating the user if necessary
        /// </summary>
        /// <returns>A valid user ID that can be used for operations</returns>
        private async Task<string> GetOrCreateUserIdAsync()
        {
            try
            {
                // First try to get user ID from RequestContext
                var userId = RequestContext.GetCurrentUserId();

                // Log if user ID not found
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("User ID not found from RequestContext");
                }

                // Get or create user based on Entra ID claims
                var adminGroupId = _configuration["AzureAd:Groups:Admin"];
                var user = await _userService.GetOrCreateCurrentUserAsync(adminGroupId);

                // Return the confirmed user ID
                return user.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreateUserIdAsync");
                throw;
            }
        }
    }
}