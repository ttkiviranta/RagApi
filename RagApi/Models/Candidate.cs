namespace RagApi.Models;

/// <summary>
/// Represents a job candidate in the recruitment system
/// </summary>
public class Candidate
{
    /// <summary>
    /// Unique identifier for the candidate
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's LinkedIn profile URL
    /// </summary>
    public string LinkedInProfile { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's current job position
    /// </summary>
    public string CurrentPosition { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's current employer
    /// </summary>
    public string CurrentCompany { get; set; } = string.Empty;

    /// <summary>
    /// Reference to candidate's resume document
    /// </summary>
    public string? ResumeDocumentId { get; set; }

    /// <summary>
    /// Reference to candidate's cover letter document
    /// </summary>
    public string? CoverLetterDocumentId { get; set; }

    /// <summary>
    /// Candidate's skills (JSON-formatted list)
    /// </summary>
    public string Skills { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's location
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// When the candidate was created in the system
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the candidate was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional reference to a user account
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Navigation property to user account (if candidate has registered)
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Navigation property to all applications submitted by this candidate
    /// </summary>
    public ICollection<Application> Applications { get; set; } = new List<Application>();

    /// <summary>
    /// Gets the candidate's full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}