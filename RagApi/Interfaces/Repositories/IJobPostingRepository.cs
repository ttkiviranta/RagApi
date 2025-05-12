// RagApi/Interfaces/Repositories/IJobPostingRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces.Repositories
{
    public interface IJobPostingRepository : IRepository<JobPosting>
    {
        Task<IEnumerable<JobPosting>> GetActiveJobPostingsAsync();
        Task<IEnumerable<Application>> GetApplicationsForJobAsync(string jobPostingId);
        Task<IEnumerable<JobPosting>> GetByDepartmentAsync(string department);
        Task<IEnumerable<JobPosting>> GetByCreatedByUserIdAsync(string userId);
    }
}

