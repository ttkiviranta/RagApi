using System.ComponentModel.DataAnnotations;

public class JobPostingCreateDto
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    public string Requirements { get; set; }

    public string Location { get; set; }

    public string Department { get; set; }

    public string EmploymentType { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string SalaryCurrency { get; set; }

    public DateTime PublishedDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    [Required]
    public string Status { get; set; }
    public string? JobPostingDocumentId { get; set; }
}

public class JobPostingUpdateDto : JobPostingCreateDto
{
}

public class JobPostingResponseDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Requirements { get; set; }
    public string Location { get; set; }
    public string Department { get; set; }
    public string EmploymentType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string SalaryCurrency { get; set; }
    public string? JobPostingDocumentId { get; set; }
    public DateTime PublishedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

