using System;
using System.ComponentModel.DataAnnotations;

public record ApplicationCreateDto
{
    [Required]
    public string CandidateId { get; init; }

    [Required]
    public string JobPostingId { get; init; }

    public string Notes { get; init; }
}

public record ApplicationStatusUpdateDto
{
    [Required]
    public string Status { get; init; }

    public string Notes { get; init; }
}

public record ApplicationResponseDto
{
    public string Id { get; init; }
    public string CandidateId { get; init; }
    public string JobPostingId { get; init; }
    public DateTime AppliedDate { get; init; }
    public string Status { get; init; }
    public decimal? MatchScore { get; init; }
    public string MatchAnalysis { get; init; }
    public string Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public CandidateResponseDto Candidate { get; init; }
    public JobPostingResponseDto JobPosting { get; init; }
}
