using System;

namespace RagApi.Models.Dto
{
    /// <summary>
    /// DTO for job matching results
    /// </summary>
    public record JobMatchResultDto
    {
        /// <summary>
        /// ID of the job posting
        /// </summary>
        public string JobPostingId { get; init; } = string.Empty;

        /// <summary>
        /// Title of the job posting or name of candidate
        /// </summary>
        public string JobTitle { get; init; } = string.Empty;

        /// <summary>
        /// Match score between 0-100
        /// </summary>
        public decimal MatchScore { get; init; }

        /// <summary>
        /// Detailed analysis of the match
        /// </summary>
        public string MatchAnalysis { get; init; } = string.Empty;

        // Parametriton konstruktori
        public JobMatchResultDto()
        {
        }

        // Täysi konstruktori
        public JobMatchResultDto(string jobPostingId, string jobTitle, decimal matchScore, string matchAnalysis)
        {
            JobPostingId = jobPostingId;
            JobTitle = jobTitle;
            MatchScore = matchScore;
            MatchAnalysis = matchAnalysis;
        }
    }
}
