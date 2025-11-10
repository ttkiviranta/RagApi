using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagApi.Models;

/// <summary>
/// Represents a document in the system that can be uploaded, processed and indexed
/// </summary>
public class Document
{
    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Original filename of the document
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Path to the document in Azure Blob Storage
    /// </summary>
    public string BlobStoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Type of document (e.g., Resume, CoverLetter, JobPosting, etc.)
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the document (e.g., application/pdf)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Reference to an entity this document belongs to (e.g., candidate, job posting)
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// JSON-formatted metadata about the document
    /// </summary>
    public string Metadata { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the document was uploaded
    /// </summary>
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID of the user who uploaded the document (nullable)
    /// </summary>
    [ForeignKey("UploadedByUser")]
    public string? UploadedByUserId { get; set; }

    /// <summary>
    /// Navigation property to the user who uploaded the document
    /// </summary>
    public virtual User? UploadedByUser { get; set; }

    /// <summary>
    /// Gets a safe display name for the document
    /// </summary>
    public string DisplayName => string.IsNullOrEmpty(FileName) ? Id : FileName;

    /// <summary>
    /// Checks if the document is a PDF
    /// </summary>
    public bool IsPdf => ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
}