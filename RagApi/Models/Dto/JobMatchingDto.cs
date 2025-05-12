using System;

namespace RagApi.Models.Dto
{
    /// <summary>
    /// DTO for job matching results
    /// </summary>
    public class JobMatchResultDto
    {
        /// <summary>
        /// ID of the job posting
        /// </summary>
        public string JobPostingId { get; set; }

        /// <summary>
        /// Title of the job posting or name of candidate
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// Match score between 0-100
        /// </summary>
        public decimal MatchScore { get; set; }

        /// <summary>
        /// Detailed analysis of the match
        /// </summary>
        public string MatchAnalysis { get; set; }
    }
}
