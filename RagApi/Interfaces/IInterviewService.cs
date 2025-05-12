using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service interface for managing interview operations
    /// </summary>
    public interface IInterviewService
    {
        /// <summary>
        /// Gets all interviews
        /// </summary>
        Task<IEnumerable<Interview>> GetAllAsync();

        /// <summary>
        /// Gets an interview by ID
        /// </summary>
        Task<Interview> GetByIdAsync(string id);

        /// <summary>
        /// Creates a new interview
        /// </summary>
        Task<Interview> CreateAsync(InterviewCreateDto dto, string interviewerId);

        /// <summary>
        /// Updates an existing interview
        /// </summary>
        Task<Interview> UpdateAsync(string id, InterviewUpdateDto dto);

        /// <summary>
        /// Gets all interviews for a specific application
        /// </summary>
        Task<IEnumerable<Interview>> GetByApplicationAsync(string applicationId);

        /// <summary>
        /// Generates interview questions based on candidate and job posting
        /// </summary>
        Task<List<string>> GenerateInterviewQuestionsAsync(string interviewId);
    }
}
