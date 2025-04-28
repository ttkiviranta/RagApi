using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service interface for managing job posting operations
    /// </summary>
    public interface IJobPostingService
    {
        /// <summary>
        /// Gets all job postings
        /// </summary>
        Task<IEnumerable<JobPosting>> GetAllAsync();

        /// <summary>
        /// Gets a job posting by ID
        /// </summary>
        Task<JobPosting> GetByIdAsync(string id);

        /// <summary>
        /// Creates a new job posting
        /// </summary>
        Task<JobPosting> CreateAsync(JobPostingCreateDto dto, string userId);

        /// <summary>
        /// Updates an existing job posting
        /// </summary>
        Task<JobPosting> UpdateAsync(string id, JobPostingUpdateDto dto);

        /// <summary>
        /// Deletes a job posting
        /// </summary>
        Task DeleteAsync(string id);

        /// <summary>
        /// Uploads a document for a job posting
        /// </summary>
        Task<string> UploadDocumentAsync(string jobPostingId, IFormFile file, string userId);

        /// <summary>
        /// Gets active job postings
        /// </summary>
        Task<IEnumerable<JobPosting>> GetActiveAsync();
    }
}
