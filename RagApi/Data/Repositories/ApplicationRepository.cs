// RagApi/Data/Repositories/ApplicationRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Interfaces.Repositories;
using RagApi.Models;

namespace RagApi.Data.Repositories
{
    public class ApplicationRepository : Repository<Application>, IApplicationRepository
    {
        public ApplicationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Application>> GetByCandidateIdAsync(string candidateId)
        {
            return await _dbSet
                .Where(a => a.CandidateId == candidateId)
                .Include(a => a.JobPosting)
                .ToListAsync();
        }

        public async Task<IEnumerable<Application>> GetByJobPostingIdAsync(string jobPostingId)
        {
            return await _dbSet
                .Where(a => a.JobPostingId == jobPostingId)
                .Include(a => a.Candidate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Application>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(a => a.Status == status)
                .ToListAsync();
        }

        public async Task UpdateStatusAsync(string applicationId, string status)
        {
            var application = await GetByIdAsync(applicationId);
            if (application != null)
            {
                application.Status = status;
                application.UpdatedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }
    }
}

