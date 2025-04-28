using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Services
{
    /// <summary>
    /// Implementation of job posting service interface
    /// </summary>
    public class JobPostingService : IJobPostingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDocumentService _documentService;

        public JobPostingService(
            ApplicationDbContext context,
            IDocumentService documentService)
        {
            _context = context;
            _documentService = documentService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<JobPosting>> GetAllAsync()
        {
            return await _context.JobPostings.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<JobPosting> GetByIdAsync(string id)
        {
            return await _context.JobPostings.FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<JobPosting> CreateAsync(JobPostingCreateDto dto, string userId)
        {
            var jobPosting = new JobPosting
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Location = dto.Location,
                Department = dto.Department,
                EmploymentType = dto.EmploymentType,
                SalaryMin = dto.SalaryMin,
                SalaryMax = dto.SalaryMax,
                SalaryCurrency = dto.SalaryCurrency,
                PublishedDate = dto.PublishedDate,
                ExpirationDate = dto.ExpirationDate,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            _context.JobPostings.Add(jobPosting);
            await _context.SaveChangesAsync();

            return jobPosting;
        }

        /// <inheritdoc/>
        public async Task<JobPosting> UpdateAsync(string id, JobPostingUpdateDto dto)
        {
            var jobPosting = await _context.JobPostings.FindAsync(id);
            if (jobPosting == null)
                return null;

            jobPosting.Title = dto.Title;
            jobPosting.Description = dto.Description;
            jobPosting.Requirements = dto.Requirements;
            jobPosting.Location = dto.Location;
            jobPosting.Department = dto.Department;
            jobPosting.EmploymentType = dto.EmploymentType;
            jobPosting.SalaryMin = dto.SalaryMin;
            jobPosting.SalaryMax = dto.SalaryMax;
            jobPosting.SalaryCurrency = dto.SalaryCurrency;
            jobPosting.PublishedDate = dto.PublishedDate;
            jobPosting.ExpirationDate = dto.ExpirationDate;
            jobPosting.Status = dto.Status;
            jobPosting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return jobPosting;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id)
        {
            var jobPosting = await _context.JobPostings.FindAsync(id);
            if (jobPosting != null)
            {
                _context.JobPostings.Remove(jobPosting);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadDocumentAsync(string jobPostingId, IFormFile file, string userId)
        {
            var jobPosting = await _context.JobPostings.FindAsync(jobPostingId);
            if (jobPosting == null)
                throw new KeyNotFoundException("Job posting not found");

            // Upload and process document
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "JobPosting",
                jobPostingId,
                userId);

            // Update job posting's document reference
            jobPosting.JobPostingDocumentId = documentId;
            jobPosting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return documentId;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<JobPosting>> GetActiveAsync()
        {
            var currentDate = DateTime.UtcNow.Date;

            return await _context.JobPostings
                .Where(jp => jp.Status == "Active" && jp.ExpirationDate >= currentDate)
                .ToListAsync();
        }
    }
}