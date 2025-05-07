using RagApi.Models;

public class JobPosting
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Requirements { get; set; }
    public string Location { get; set; }
    public string Department { get; set; }
    public string EmploymentType { get; set; } // Full-time, Part-time, Contract, etc.
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }
    public string? JobPostingDocumentId { get; set; } // Viiteavain dokumenttiin
    public DateTime PublishedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Status { get; set; } // Active, Closed, Draft
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByUserId { get; set; } // Viiteavain Users-tauluun

    // Navigointiproperties
    public User CreatedByUser { get; set; }
    public ICollection<Application> Applications { get; set; }
}
