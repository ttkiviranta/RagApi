using System;
using System.Collections.Generic;
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
        // In JobPostingService.cs, modify the CreateAsync method:

        public async Task<JobPosting> CreateAsync(JobPostingCreateDto dto, string? userId)
        {
            var jobPosting = new JobPosting
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                Requirements = dto.Requirements ?? string.Empty,
                Location = dto.Location ?? string.Empty,
                Department = dto.Department ?? string.Empty,
                EmploymentType = dto.EmploymentType ?? string.Empty,
                SalaryMin = dto.SalaryMin,
                SalaryMax = dto.SalaryMax,
                SalaryCurrency = dto.SalaryCurrency,
                JobPostingDocumentId = dto.JobPostingDocumentId,
                PublishedDate = dto.PublishedDate,
                ExpirationDate = dto.ExpirationDate,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = userId // Don't use a default value - let it be null if userId is null
            };

            // If document ID is provided, update the document entity reference
            if (!string.IsNullOrEmpty(dto.JobPostingDocumentId))
            {
                var document = await _context.Documents.FindAsync(dto.JobPostingDocumentId);
                if (document != null)
                {
                    document.EntityId = jobPosting.Id;
                    // Also update document's state
                    _context.Documents.Update(document);
                }
            }

            _context.JobPostings.Add(jobPosting);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details
                throw new Exception($"Failed to create job posting: {ex.InnerException?.Message}", ex);
            }

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
            // Only update document ID if provided
            if (!string.IsNullOrEmpty(dto.JobPostingDocumentId))
            {
                jobPosting.JobPostingDocumentId = dto.JobPostingDocumentId;
            }
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
            else
            {
                throw new KeyNotFoundException($"Job posting with ID {id} not found");
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadDocumentAsync(string jobPostingId, IFormFile file, string? userId)
        {
            var jobPosting = await _context.JobPostings.FindAsync(jobPostingId);
            if (jobPosting == null)
                throw new KeyNotFoundException("Job posting not found");

            // Upload and process document
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "JobPosting",
                jobPostingId,
                userId); // userId can be null

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
