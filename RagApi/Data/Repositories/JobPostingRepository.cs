// RagApi/Data/Repositories/JobPostingRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Interfaces.Repositories;
using RagApi.Models;

namespace RagApi.Data.Repositories
{
    public class JobPostingRepository : Repository<JobPosting>, IJobPostingRepository
    {
        public JobPostingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<JobPosting>> GetActiveJobPostingsAsync()
        {
            var currentDate = DateTime.UtcNow.Date;
            return await _dbSet
                .Where(jp => jp.Status == "Active" && jp.ExpirationDate >= currentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Application>> GetApplicationsForJobAsync(string jobPostingId)
        {
            return await _context.Applications
                .Where(a => a.JobPostingId == jobPostingId)
                .Include(a => a.Candidate)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetByDepartmentAsync(string department)
        {
            return await _dbSet
                .Where(jp => jp.Department == department)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobPosting>> GetByCreatedByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(jp => jp.CreatedByUserId == userId)
                .ToListAsync();
        }
    }
}

