namespace RagApi.Models;

/// <summary>
/// Represents a job posting in the recruitment system
/// </summary>
public class JobPosting
{
    /// <summary>
    /// Unique identifier for the job posting
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of the job posting
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the job
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Requirements for the job
    /// </summary>
    public string Requirements { get; set; } = string.Empty;

    /// <summary>
    /// Location where the job is based
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Department within the company
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Type of employment (Full-time, Part-time, Contract, etc.)
    /// </summary>
    public string EmploymentType { get; set; } = string.Empty;

    /// <summary>
    /// Minimum salary offered
    /// </summary>
    public decimal? SalaryMin { get; set; }

    /// <summary>
    /// Maximum salary offered
    /// </summary>
    public decimal? SalaryMax { get; set; }

    /// <summary>
    /// Currency for the salary
    /// </summary>
    public string? SalaryCurrency { get; set; }

    /// <summary>
    /// Reference to detailed job posting document
    /// </summary>
    public string? JobPostingDocumentId { get; set; }

    /// <summary>
    /// Date when the job posting was published
    /// </summary>
    public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the job posting expires
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Current status of the job posting (Active, Closed, Draft)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When the job posting was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the job posting was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID of the user who created this job posting
    /// </summary>
    public string? CreatedByUserId { get; set; }

    /// <summary>
    /// Navigation property to the creator user
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Navigation property to applications for this job posting
    /// </summary>
    public ICollection<Application> Applications { get; set; } = new List<Application>();

    /// <summary>
    /// Returns a formatted salary range string
    /// </summary>
    public string GetSalaryRangeText() => SalaryMin.HasValue && SalaryMax.HasValue
        ? $"{SalaryMin}-{SalaryMax} {SalaryCurrency ?? "EUR"}"
        : "Salary not specified";

    /// <summary>
    /// Checks if the job posting is active
    /// </summary>
    public bool IsActive => Status == "Active" && DateTime.UtcNow < ExpirationDate;
}
