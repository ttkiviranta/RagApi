using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDocumentService _documentService;

        public JobPostingService(
            IUnitOfWork unitOfWork,
            IDocumentService documentService)
        {
            _unitOfWork = unitOfWork;
            _documentService = documentService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<JobPosting>> GetAllAsync()
        {
            return await _unitOfWork.JobPostings.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<JobPosting?> GetByIdAsync(string id)
        {
            return await _unitOfWork.JobPostings.GetByIdAsync(id);
        }

        /// <inheritdoc/>
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
                CreatedByUserId = userId
            };

            try
            {
                // Jos dokumentti-ID on annettu, päivitä dokumentin entityId-viittaus
                if (!string.IsNullOrEmpty(dto.JobPostingDocumentId))
                {
                    var document = await _unitOfWork.Documents.GetByIdAsync(dto.JobPostingDocumentId);
                    if (document != null)
                    {
                        document.EntityId = jobPosting.Id;
                        await _unitOfWork.Documents.UpdateAsync(document);
                    }
                }

                await _unitOfWork.JobPostings.AddAsync(jobPosting);
                await _unitOfWork.CommitAsync();

                return jobPosting;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create job posting: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<JobPosting?> UpdateAsync(string id, JobPostingUpdateDto dto)
        {
            var jobPosting = await _unitOfWork.JobPostings.GetByIdAsync(id);
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

            await _unitOfWork.JobPostings.UpdateAsync(jobPosting);
            await _unitOfWork.CommitAsync();

            return jobPosting;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id)
        {
            var jobPosting = await _unitOfWork.JobPostings.GetByIdAsync(id);
            if (jobPosting != null)
            {
                await _unitOfWork.JobPostings.DeleteAsync(jobPosting);
                await _unitOfWork.CommitAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Job posting with ID {id} not found");
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadDocumentAsync(string jobPostingId, IFormFile file, string? userId)
        {
            var jobPosting = await _unitOfWork.JobPostings.GetByIdAsync(jobPostingId);
            if (jobPosting == null)
                throw new KeyNotFoundException("Job posting not found");

            // Käytä dokumenttipalvelua lataamaan dokumentti
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "JobPosting",
                jobPostingId,
                userId);

            // Päivitä työilmoituksen dokumenttiviite
            jobPosting.JobPostingDocumentId = documentId;
            jobPosting.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.JobPostings.UpdateAsync(jobPosting);
            await _unitOfWork.CommitAsync();

            return documentId;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<JobPosting>> GetActiveAsync()
        {
            return await _unitOfWork.JobPostings.GetActiveJobPostingsAsync();
        }
    }
}
