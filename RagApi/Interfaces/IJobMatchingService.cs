using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service interface for job matching operations
    /// </summary>
    public interface IJobMatchingService
    {
        /// <summary>
        /// Analyzes match between a candidate and job posting
        /// </summary>
        Task<JobMatchResult> AnalyzeJobMatchAsync(string resumeBlobPath, string jobPostingBlobPath);

        /// <summary>
        /// Generates interview questions based on candidate resume and job posting
        /// </summary>
        Task<List<string>> GenerateInterviewQuestionsAsync(string resumeBlobPath, string jobPostingBlobPath);

        /// <summary>
        /// Analyzes a job application
        /// </summary>
        Task<string> AnalyzeApplicationAsync(string applicationId);

        /// <summary>
        /// Matches a candidate with suitable job postings
        /// </summary>
        Task<List<JobMatchResultDto>> MatchCandidateWithJobsAsync(string candidateId, int limit = 10);

        /// <summary>
        /// Matches a job posting with suitable candidates
        /// </summary>
        Task<List<JobMatchResultDto>> MatchJobWithCandidatesAsync(string jobPostingId, int limit = 10);
    }
}
