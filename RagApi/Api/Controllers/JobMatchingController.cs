using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RagApi.Api.Models;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobMatchingController : ControllerBase
    {
        private readonly IRagService _ragService;
        private readonly ISystemPromptService _systemPromptService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IJobMatchingService _jobMatchingService;

        public JobMatchingController(
            IRagService ragService,
            ISystemPromptService systemPromptService,
            ApplicationDbContext dbContext,
            IJobMatchingService jobMatchingService)
        {
            _ragService = ragService;
            _systemPromptService = systemPromptService;
            _dbContext = dbContext;
            _jobMatchingService = jobMatchingService;
        }

        /// <summary>
        /// Upload a job posting
        /// </summary>
        [HttpPost("job-postings")]
        public async Task<IActionResult> UploadJobPosting(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = true, message = "No file was uploaded." });
            }

            // Currently only supporting PDF files
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = true, message = "Only PDF files are supported at this time." });
            }

            try
            {
                using var stream = file.OpenReadStream();
                string documentId = await _ragService.ProcessPdfAsync(stream, file.FileName);

                // Store metadata to indicate this is a job posting
                // In a real implementation, we would store this in a database

                return Ok(new
                {
                    error = false,
                    data = new UploadDocumentResponse
                    {
                        DocumentId = documentId,
                        Message = "Job posting processed successfully."
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error processing job posting: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload a candidate's resume
        /// </summary>
        [HttpPost("candidates/{candidateId}/resume")]
        public async Task<IActionResult> UploadCandidateResume(string candidateId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = true, message = "No file was uploaded." });
            }

            // Currently only supporting PDF files
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = true, message = "Only PDF files are supported at this time." });
            }

            try
            {
                using var stream = file.OpenReadStream();
                string documentId = await _ragService.ProcessPdfAsync(stream, file.FileName);

                // Store metadata to associate this resume with the candidate
                // In a real implementation, we would store this in a database

                return Ok(new
                {
                    error = false,
                    data = new UploadDocumentResponse
                    {
                        DocumentId = documentId,
                        Message = $"Resume for candidate {candidateId} processed successfully."
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error processing resume: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload a candidate's cover letter
        /// </summary>
        [HttpPost("candidates/{candidateId}/cover-letter")]
        public async Task<IActionResult> UploadCandidateCoverLetter(string candidateId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = true, message = "No file was uploaded." });
            }

            // Currently only supporting PDF files
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = true, message = "Only PDF files are supported at this time." });
            }

            try
            {
                using var stream = file.OpenReadStream();
                string documentId = await _ragService.ProcessPdfAsync(stream, file.FileName);

                // Store metadata to associate this cover letter with the candidate
                // In a real implementation, we would store this in a database

                return Ok(new
                {
                    error = false,
                    data = new UploadDocumentResponse
                    {
                        DocumentId = documentId,
                        Message = $"Cover letter for candidate {candidateId} processed successfully."
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error processing cover letter: {ex.Message}" });
            }
        }

        /// <summary>
        /// Match job posting with candidate resumes and cover letters
        /// </summary>
        [HttpPost("match")]
        public async Task<IActionResult> MatchJobWithCandidates([FromBody] JobMatchingRequest request)
        {
            if (string.IsNullOrEmpty(request?.JobPosting))
            {
                return BadRequest(new { error = true, message = "Job posting cannot be empty." });
            }

            try
            {
                // Ensure we're using the HR job matching prompt
                var hrPrompt = await _systemPromptService.GetSystemPromptsAsync();
                var jobMatchingPrompt = hrPrompt.FirstOrDefault(p => p.Name == "HR Job Matching");

                string userId = null;

                // If we found the HR prompt, create a temporary user with it as default
                if (jobMatchingPrompt != null)
                {
                    // In a real implementation, we would use a more sophisticated method
                    // Here we're simplifying by creating a temporary user just for this request
                    var tempUser = new User
                    {
                        Username = "JobMatchingTemp",
                        Email = $"temp-{Guid.NewGuid()}@example.com"
                    };

                    _dbContext.Users.Add(tempUser);
                    await _dbContext.SaveChangesAsync();

                    await _systemPromptService.AssignSystemPromptToUserAsync(
                        tempUser.Id,
                        jobMatchingPrompt.Id,
                        true);

                    userId = tempUser.Id;
                }

                // Build a query with the job posting
                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.AppendLine("Job Description:");
                queryBuilder.AppendLine(request.JobPosting);
                queryBuilder.AppendLine();
                queryBuilder.AppendLine("Please analyze the above job description and find the best matching candidates from the available resumes and cover letters.");

                if (request.CandidateIds != null && request.CandidateIds.Count > 0)
                {
                    queryBuilder.AppendLine();
                    queryBuilder.AppendLine("Consider only the following candidates:");
                    foreach (var candidateId in request.CandidateIds)
                    {
                        queryBuilder.AppendLine($"- Candidate ID: {candidateId}");
                    }
                }

                // Execute the query
                var response = await _ragService.QueryAsync(queryBuilder.ToString(), userId);

                // Clean up temporary user
                if (userId != null)
                {
                    var tempUser = await _dbContext.Users.FindAsync(userId);
                    if (tempUser != null)
                    {
                        _dbContext.Users.Remove(tempUser);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                return Ok(new { error = false, data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error processing job matching request: {ex.Message}" });
            }
        }

        /// <summary>
        /// Suggest interview questions based on job and candidate match
        /// </summary>
        [HttpPost("interview-questions")]
        public async Task<IActionResult> GenerateInterviewQuestions([FromBody] InterviewQuestionsRequest request)
        {
            if (string.IsNullOrEmpty(request?.JobPosting))
            {
                return BadRequest(new { error = true, message = "Job posting cannot be empty." });
            }

            if (string.IsNullOrEmpty(request?.CandidateId))
            {
                return BadRequest(new { error = true, message = "Candidate ID cannot be empty." });
            }

            try
            {
                // Build a query to generate interview questions
                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.AppendLine("Job Description:");
                queryBuilder.AppendLine(request.JobPosting);
                queryBuilder.AppendLine();
                queryBuilder.AppendLine($"Candidate ID: {request.CandidateId}");
                queryBuilder.AppendLine();
                queryBuilder.AppendLine("Based on the job description and the candidate's resume and cover letter, please generate a list of targeted interview questions that will help assess the candidate's fit for this role. Include questions that:");
                queryBuilder.AppendLine("1. Verify their experience with key technical skills required");
                queryBuilder.AppendLine("2. Assess their experience with relevant projects");
                queryBuilder.AppendLine("3. Probe potential gaps in their qualifications");
                queryBuilder.AppendLine("4. Evaluate cultural fit and soft skills");

                // Execute the query
                var response = await _ragService.QueryAsync(queryBuilder.ToString());

                return Ok(new { error = false, data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error generating interview questions: {ex.Message}" });
            }
        }

        /// <summary>
        /// Match a candidate with job postings
        /// </summary>
        [HttpPost("candidates/{candidateId}/match-jobs")]
        public async Task<IActionResult> MatchCandidateWithJobs(string candidateId, [FromQuery] int limit = 10)
        {
            try
            {
                var matchResults = await _jobMatchingService.MatchCandidateWithJobsAsync(candidateId, limit);
                return Ok(new { error = false, data = matchResults });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Candidate not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error matching jobs: {ex.Message}" });
            }
        }

        /// <summary>
        /// Match a job posting with candidates
        /// </summary>
        [HttpPost("jobs/{jobPostingId}/match-candidates")]
        public async Task<IActionResult> MatchJobWithCandidates(string jobPostingId, [FromQuery] int limit = 10)
        {
            try
            {
                var matchResults = await _jobMatchingService.MatchJobWithCandidatesAsync(jobPostingId, limit);
                return Ok(new { error = false, data = matchResults });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Job posting not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error matching candidates: {ex.Message}" });
            }
        }

        /// <summary>
        /// Analyze an application
        /// </summary>
        [HttpPost("applications/{applicationId}/analyze")]
        public async Task<IActionResult> AnalyzeApplication(string applicationId)
        {
            try
            {
                var analysis = await _jobMatchingService.AnalyzeApplicationAsync(applicationId);
                return Ok(new { error = false, data = analysis });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Application not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error analyzing application: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// Request for generating interview questions for a candidate
    /// </summary>
    public class InterviewQuestionsRequest
    {
        /// <summary>
        /// Job posting text or ID
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public string JobPosting { get; set; }

        /// <summary>
        /// Candidate ID
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public string CandidateId { get; set; }
    }
}