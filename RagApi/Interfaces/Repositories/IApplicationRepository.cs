// RagApi/Interfaces/Repositories/IApplicationRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces.Repositories
{
    public interface IApplicationRepository : IRepository<Application>
    {
        Task<IEnumerable<Application>> GetByCandidateIdAsync(string candidateId);
        Task<IEnumerable<Application>> GetByJobPostingIdAsync(string jobPostingId);
        Task<IEnumerable<Application>> GetByStatusAsync(string status);
        Task UpdateStatusAsync(string applicationId, string status);
    }
}

