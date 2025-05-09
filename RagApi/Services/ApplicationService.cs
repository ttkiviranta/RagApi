using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJobMatchingService _jobMatchingService;

        public ApplicationService(
            IUnitOfWork unitOfWork,
            IJobMatchingService jobMatchingService)
        {
            _unitOfWork = unitOfWork;
            _jobMatchingService = jobMatchingService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetAllAsync()
        {
            return await _unitOfWork.Applications.GetAllAsync();
            // Huomaa: Mukaan pitäisi liittää navigationPropertit LoadWith-käskyillä
            // tai tehdä erillinen metodi repositoryyn
        }

        /// <inheritdoc/>
        public async Task<Application> GetByIdAsync(string id)
        {
            return await _unitOfWork.Applications.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Application> CreateAsync(ApplicationCreateDto dto)
        {
            // Verify candidate and job posting exist
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(dto.CandidateId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            var jobPosting = await _unitOfWork.JobPostings.GetByIdAsync(dto.JobPostingId);
            if (jobPosting == null)
                throw new KeyNotFoundException("Job posting not found");

            // Check if candidate has already applied to this job
            var existingApplications = await _unitOfWork.Applications.FindAsync(a =>
                a.CandidateId == dto.CandidateId && a.JobPostingId == dto.JobPostingId);

            if (existingApplications.Any())
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

            await _unitOfWork.Applications.AddAsync(application);
            await _unitOfWork.CommitAsync();

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
            var application = await _unitOfWork.Applications.GetByIdAsync(id);
            if (application == null)
                return null;

            application.Status = dto.Status;
            application.Notes = dto.Notes;
            application.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Applications.UpdateAsync(application);
            await _unitOfWork.CommitAsync();

            return application;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetByJobPostingAsync(string jobPostingId)
        {
            return await _unitOfWork.Applications.GetByJobPostingIdAsync(jobPostingId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Application>> GetByCandidateAsync(string candidateId)
        {
            return await _unitOfWork.Applications.GetByCandidateIdAsync(candidateId);
        }

        /// <inheritdoc/>
        public async Task<JobMatchResult> GenerateMatchScoreAsync(string applicationId)
        {
            var application = await _unitOfWork.Applications.GetByIdAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException("Application not found");

            // Get candidate resume documents
            var candidateDocuments = await _unitOfWork.Documents.FindAsync(d =>
                d.EntityId == application.CandidateId && d.DocumentType == "Resume");

            // Get job posting documents
            var jobDocuments = await _unitOfWork.Documents.FindAsync(d =>
                d.EntityId == application.JobPostingId && d.DocumentType == "JobPosting");

            if (!candidateDocuments.Any() || !jobDocuments.Any())
                throw new InvalidOperationException("Required documents not found");

            // Use job matching service to analyze match
            var resumeDocument = candidateDocuments.First();
            var jobPostingDocument = jobDocuments.First();

            var matchResult = await _jobMatchingService.AnalyzeJobMatchAsync(
                resumeDocument.BlobStoragePath,
                jobPostingDocument.BlobStoragePath);

            // Update application match score and analysis
            application.MatchScore = matchResult.Score;
            application.MatchAnalysis = matchResult.Analysis;
            application.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Applications.UpdateAsync(application);
            await _unitOfWork.CommitAsync();

            return matchResult;
        }
    }
}
