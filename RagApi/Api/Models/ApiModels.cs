using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RagApi.Api.Models
{
    /// <summary>
    /// Request to upload a PDF document
    /// </summary>
    public class UploadDocumentRequest
    {
        /// <summary>
        /// Type of document being uploaded
        /// </summary>
        [Required]
        public string DocumentType { get; set; } // Resume, CoverLetter, JobPosting, etc.

        /// <summary>
        /// Optional ID to associate the document with a specific entity (e.g., candidate ID)
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Optional metadata about the document
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Response after uploading a document
    /// </summary>
    public class UploadDocumentResponse
    {
        /// <summary>
        /// Document identifier
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Request for making a query to the RAG system
    /// </summary>
    public class QueryRequest
    {
        /// <summary>
        /// User's query text
        /// </summary>
        [Required]
        public string Query { get; set; }

        /// <summary>
        /// Optional filters to apply to the search
        /// </summary>
        public Dictionary<string, string> Filters { get; set; }

        /// <summary>
        /// Maximum number of documents to retrieve
        /// </summary>
        public int MaxResults { get; set; } = 5;
    }

    /// <summary>
    /// Request to create a new conversation
    /// </summary>
    public class CreateConversationRequest
    {
        /// <summary>
        /// Title of the conversation
        /// </summary>
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }
    }

    /// <summary>
    /// Request to create a new system prompt
    /// </summary>
    public class CreateSystemPromptRequest
    {
        /// <summary>
        /// Name of the prompt
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Description of the prompt
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// The actual prompt text
        /// </summary>
        [Required]
        public string PromptText { get; set; }

        /// <summary>
        /// Whether this is the default prompt
        /// </summary>
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Request to update an existing system prompt
    /// </summary>
    public class UpdateSystemPromptRequest
    {
        /// <summary>
        /// Updated name of the prompt
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Updated description of the prompt
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Updated prompt text
        /// </summary>
        [Required]
        public string PromptText { get; set; }

        /// <summary>
        /// Updated default status
        /// </summary>
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Request to assign a system prompt to a user
    /// </summary>
    public class AssignSystemPromptRequest
    {
        /// <summary>
        /// Whether this should be the user's default prompt
        /// </summary>
        public bool IsDefault { get; set; } = true;
    }

    /// <summary>
    /// Request to create a new user
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Username for the new user
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }
    }

    /// <summary>
    /// Request for job matching analysis
    /// </summary>
    public class JobMatchingRequest
    {
        /// <summary>
        /// Job posting text or ID
        /// </summary>
        [Required]
        public string JobPosting { get; set; }

        /// <summary>
        /// Optional filter by candidate IDs
        /// </summary>
        public List<string> CandidateIds { get; set; }

        /// <summary>
        /// Maximum number of candidates to include in results
        /// </summary>
        public int MaxCandidates { get; set; } = 10;
    }

    /// <summary>
    /// Response model for API version information
    /// </summary>
    public class VersionInfoResponse
    {
        /// <summary>
        /// Version number from AssemblyVersion
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Product name from AssemblyProduct attribute
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Company name from AssemblyCompany attribute
        /// </summary>
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// Build date if available
        /// </summary>
        public DateTime? BuildDate { get; set; }

        /// <summary>
        /// Current environment name
        /// </summary>
        public string Environment { get; set; } = string.Empty;
    }
}
