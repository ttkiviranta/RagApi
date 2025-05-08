using RagApi.Models;

public class JobPosting
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty; // Full-time, Part-time, Contract, etc.
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }
    public string? JobPostingDocumentId { get; set; } // Viiteavain dokumenttiin
    public DateTime PublishedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Closed, Draft
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; } // Viiteavain Users-tauluun, nullable

    // Navigointiproperties
    public User? CreatedByUser { get; set; }
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
