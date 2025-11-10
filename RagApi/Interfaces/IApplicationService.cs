using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service interface for managing job application operations
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Gets all applications
        /// </summary>
        Task<IEnumerable<Application>> GetAllAsync();

        /// <summary>
        /// Gets an application by ID
        /// </summary>
        Task<Application> GetByIdAsync(string id);

        /// <summary>
        /// Creates a new application
        /// </summary>
        Task<Application> CreateAsync(ApplicationCreateDto dto);

        /// <summary>
        /// Updates an application's status
        /// </summary>
        Task<Application> UpdateStatusAsync(string id, ApplicationStatusUpdateDto dto);

        /// <summary>
        /// Gets all applications for a specific job posting
        /// </summary>
        Task<IEnumerable<Application>> GetByJobPostingAsync(string jobPostingId);

        /// <summary>
        /// Gets all applications for a specific candidate
        /// </summary>
        Task<IEnumerable<Application>> GetByCandidateAsync(string candidateId);

        /// <summary>
        /// Analyzes match between candidate and job posting
        /// </summary>
        Task<JobMatchResult> GenerateMatchScoreAsync(string applicationId);
    }
}
