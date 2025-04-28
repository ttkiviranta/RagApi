using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Services
{
    /// <summary>
    /// Implementation of application service interface
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobMatchingService _jobMatchingService;

        public ApplicationService(
            ApplicationDbContext context,
            IJobMatchingService jobMatchingService)
        {
            _context = context;
            _jobMatchingService = jobMatchingService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetAllAsync()
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.JobPosting)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Application> GetByIdAsync(string id)
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Application> CreateAsync(ApplicationCreateDto dto)
        {
            // Verify candidate and job posting exist
            var candidate = await _context.Candidates.FindAsync(dto.CandidateId);
            var jobPosting = await _context.JobPostings.FindAsync(dto.JobPostingId);

            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            if (jobPosting == null)
                throw new KeyNotFoundException("Job posting not found");

            // Check if candidate has already applied to this job
            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(a => a.CandidateId == dto.CandidateId && a.JobPostingId == dto.JobPostingId);

            if (existingApplication != null)
                throw new InvalidOperationException("Candidate has already applied to this job posting");

            var application = new Application
            {
                Id = Guid.NewGuid().ToString(),
                CandidateId = dto.CandidateId,
                JobPostingId = dto.JobPostingId,
                AppliedDate = DateTime.UtcNow,
                Status = "New",
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            // Generate match score asynchronously
            _ = Task.Run(async () => {
                try
                {
                    await GenerateMatchScoreAsync(application.Id);
                }
                catch (Exception)
                {
                    // Log error but don't fail the request
                }
            });

            return application;
        }

        /// <inheritdoc/>
        public async Task<Application> UpdateStatusAsync(string id, ApplicationStatusUpdateDto dto)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                return null;

            application.Status = dto.Status;
            application.Notes = dto.Notes;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return application;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetByJobPostingAsync(string jobPostingId)
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                .Where(a => a.JobPostingId == jobPostingId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetByCandidateAsync(string candidateId)
        {
            return await _context.Applications
                .Include(a => a.JobPosting)
                .Where(a => a.CandidateId == candidateId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<JobMatchResult> GenerateMatchScoreAsync(string applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new KeyNotFoundException("Application not found");

            // Get candidate resume and job posting documents
            var candidateDocuments = await _context.Documents
                .Where(d => d.EntityId == application.CandidateId && d.DocumentType == "Resume")
                .ToListAsync();

            var jobDocuments = await _context.Documents
                .Where(d => d.EntityId == application.JobPostingId && d.DocumentType == "JobPosting")
                .ToListAsync();

            if (!candidateDocuments.Any() || !jobDocuments.Any())
                throw new InvalidOperationException("Required documents not found");

            // Use job matching service to analyze match
            var resumeDocument = candidateDocuments.FirstOrDefault();
            var jobPostingDocument = jobDocuments.FirstOrDefault();

            var matchResult = await _jobMatchingService.AnalyzeJobMatchAsync(
                resumeDocument.BlobStoragePath,
                jobPostingDocument.BlobStoragePath);

            // Update application match score and analysis
            application.MatchScore = matchResult.Score;
            application.MatchAnalysis = matchResult.Analysis;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return matchResult;
        }
    }
}
