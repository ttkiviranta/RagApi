using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service interface for managing candidate operations
    /// </summary>
    public interface ICandidateService
    {
        /// <summary>
        /// Gets all candidates
        /// </summary>
        Task<IEnumerable<Candidate>> GetAllAsync();

        /// <summary>
        /// Gets a candidate by ID
        /// </summary>
        Task<Candidate> GetByIdAsync(string id);

        /// <summary>
        /// Creates a new candidate
        /// </summary>
        Task<Candidate> CreateAsync(CandidateCreateDto dto, string? userId);

        /// <summary>
        /// Updates an existing candidate
        /// </summary>
        Task<Candidate> UpdateAsync(string id, CandidateUpdateDto dto);

        /// <summary>
        /// Deletes a candidate
        /// </summary>
        Task DeleteAsync(string id);

        /// <summary>
        /// Uploads a resume for a candidate
        /// </summary>
        Task<string> UploadResumeAsync(string candidateId, IFormFile file, string? userId);

        /// <summary>
        /// Uploads a cover letter for a candidate
        /// </summary>
        Task<string> UploadCoverLetterAsync(string candidateId, IFormFile file, string? userId);
    }
}
