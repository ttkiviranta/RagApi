using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Services
{
    /// <summary>
    /// Implementation of interview service interface
    /// </summary>
    public class InterviewService : IInterviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobMatchingService _jobMatchingService;

        public InterviewService(
            ApplicationDbContext context,
            IJobMatchingService jobMatchingService)
        {
            _context = context;
            _jobMatchingService = jobMatchingService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Interview>> GetAllAsync()
        {
            return await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Candidate)
                .Include(i => i.Application)
                    .ThenInclude(a => a.JobPosting)
                .Include(i => i.Interviewer)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Interview> GetByIdAsync(string id)
        {
            return await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Candidate)
                .Include(i => i.Application)
                    .ThenInclude(a => a.JobPosting)
                .Include(i => i.Interviewer)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Interview> CreateAsync(InterviewCreateDto dto, string interviewerId)
        {
            var application = await _context.Applications.FindAsync(dto.ApplicationId);
            if (application == null)
                throw new KeyNotFoundException("Application not found");

            var interview = new Interview
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationId = dto.ApplicationId,
                ScheduledDate = dto.ScheduledDate,
                InterviewerId = interviewerId,
                InterviewType = dto.InterviewType,
                Status = "Scheduled",
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();

            // Generate interview questions asynchronously
            _ = Task.Run(async () => {
                try
                {
                    await GenerateInterviewQuestionsAsync(interview.Id);
                }
                catch (Exception)
                {
                    // Log error but don't fail the request
                }
            });

            return interview;
        }

        /// <inheritdoc/>
        public async Task<Interview> UpdateAsync(string id, InterviewUpdateDto dto)
        {
            var interview = await _context.Interviews.FindAsync(id);
            if (interview == null)
                return null;

            interview.ScheduledDate = dto.ScheduledDate;
            interview.InterviewType = dto.InterviewType;
            interview.Status = dto.Status;
            interview.Notes = dto.Notes;
            interview.Feedback = dto.Feedback;
            interview.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return interview;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Interview>> GetByApplicationAsync(string applicationId)
        {
            return await _context.Interviews
                .Include(i => i.Interviewer)
                .Where(i => i.ApplicationId == applicationId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<string>> GenerateInterviewQuestionsAsync(string interviewId)
        {
            var interview = await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Candidate)
                .Include(i => i.Application)
                    .ThenInclude(a => a.JobPosting)
                .FirstOrDefaultAsync(i => i.Id == interviewId);

            if (interview == null)
                throw new KeyNotFoundException("Interview not found");

            // Get candidate resume and job posting documents
            var candidateDocuments = await _context.Documents
                .Where(d => d.EntityId == interview.Application.CandidateId && d.DocumentType == "Resume")
                .ToListAsync();

            var jobDocuments = await _context.Documents
                .Where(d => d.EntityId == interview.Application.JobPostingId && d.DocumentType == "JobPosting")
                .ToListAsync();

            if (!candidateDocuments.Any() || !jobDocuments.Any())
                throw new InvalidOperationException("Required documents not found");

            // Use job matching service to generate interview questions
            var resumeDocument = candidateDocuments.FirstOrDefault();
            var jobPostingDocument = jobDocuments.FirstOrDefault();

            var questions = await _jobMatchingService.GenerateInterviewQuestionsAsync(
                resumeDocument.BlobStoragePath,
                jobPostingDocument.BlobStoragePath);

            // Update interview questions
            interview.Questions = JsonSerializer.Serialize(questions);
            interview.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return questions;
        }
    }
}