using System.ComponentModel.DataAnnotations;

public class ApplicationCreateDto
{
    [Required]
    public string CandidateId { get; set; }

    [Required]
    public string JobPostingId { get; set; }

    public string Notes { get; set; }
}

public class ApplicationStatusUpdateDto
{
    [Required]
    public string Status { get; set; }

    public string Notes { get; set; }
}

public class ApplicationResponseDto
{
    public string Id { get; set; }
    public string CandidateId { get; set; }
    public string JobPostingId { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; }
    public decimal? MatchScore { get; set; }
    public string MatchAnalysis { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigaatiokohteiden tiedot
    public CandidateResponseDto Candidate { get; set; }
    public JobPostingResponseDto JobPosting { get; set; }
}
