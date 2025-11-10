using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RagApi.Api.Models;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobMatchingController : BaseController
    {
        private readonly IRagService _ragService;
        private readonly ISystemPromptService _systemPromptService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IJobMatchingService _jobMatchingService;
        private readonly ILogger<JobMatchingController> _logger;
        private readonly JobPostingMapper _jobPostingMapper;

        public JobMatchingController(
            IRagService ragService,
            ISystemPromptService systemPromptService,
            ApplicationDbContext dbContext,
            IJobMatchingService jobMatchingService,
            IRequestContext requestContext,
            ILogger<JobMatchingController> logger,
            JobPostingMapper jobPostingMapper)
            : base(requestContext)
        {
            _ragService = ragService;
            _systemPromptService = systemPromptService;
            _dbContext = dbContext;
            _jobMatchingService = jobMatchingService;
            _logger = logger;
            _jobPostingMapper = jobPostingMapper;
        }

        [HttpPost("job-postings")]
        public async Task<IActionResult> UploadJobPosting(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequestError("No file was uploaded.");
            }

            // Only PDF files are supported
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequestError("Only PDF files are supported at this time.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                string documentId = await _ragService.ProcessPdfAsync(stream, file.FileName);

                // Store metadata to indicate this is a job posting

                return Success(new UploadDocumentResponse
                {
                    DocumentId = documentId,
                    Message = "Job posting processed successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job posting file {FileName}", file.FileName);
                return Error($"Error processing job posting: {ex.Message}");
            }
        }

        [HttpPost("candidates/{candidateId}/resume")]
        public async Task<IActionResult> UploadCandidateResume(string candidateId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequestError("No file was uploaded.");
            }

            // Only PDF files are supported
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequestError("Only PDF files are supported at this time.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                string documentId = await _ragService.ProcessPdfAsync(stream, file.FileName);

                // Store metadata to associate this resume with the candidate

                return Success(new UploadDocumentResponse
                {
                    DocumentId = documentId,
                    Message = $"Resume for candidate {candidateId} processed successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing resume for candidate {CandidateId}", candidateId);
                return Error($"Error processing resume: {ex.Message}");
            }
        }

        [HttpPost("candidates/{candidateId}/cover-letter")]
        public async Task<IActionResult> UploadCandidateCoverLetter(string candidateId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequestError("No file was uploaded.");
            }

            // Only PDF files are supported
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequestError("Only PDF files are supported at this time.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                string documentId = await _ragService.ProcessPdfAsync(stream, file.FileName);

                // Store metadata to associate this cover letter with the candidate

                return Success(new UploadDocumentResponse
                {
                    DocumentId = documentId,
                    Message = $"Cover letter for candidate {candidateId} processed successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cover letter for candidate {CandidateId}", candidateId);
                return Error($"Error processing cover letter: {ex.Message}");
            }
        }

        [HttpPost("match")]
        public async Task<IActionResult> MatchJobWithCandidates([FromBody] JobMatchingRequest request)
        {
            if (string.IsNullOrEmpty(request?.JobPosting))
            {
                return BadRequestError("Job posting cannot be empty.");
            }

            try
            {
                var hrPrompt = await _systemPromptService.GetSystemPromptsAsync();
                var jobMatchingPrompt = hrPrompt.FirstOrDefault(p => p.Name == "HR Job Matching");

                string userId = null;

                if (jobMatchingPrompt != null)
                {
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

                var response = await _ragService.QueryAsync(queryBuilder.ToString(), userId);

                if (userId != null)
                {
                    var tempUser = await _dbContext.Users.FindAsync(userId);
                    if (tempUser != null)
                    {
                        _dbContext.Users.Remove(tempUser);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                return Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job matching request");
                return Error($"Error processing job matching request: {ex.Message}");
            }
        }

        [HttpPost("interview-questions")]
        public async Task<IActionResult> GenerateInterviewQuestions([FromBody] InterviewQuestionsRequest request)
        {
            if (string.IsNullOrEmpty(request?.JobPosting))
            {
                return BadRequestError("Job posting cannot be empty.");
            }

            if (string.IsNullOrEmpty(request?.CandidateId))
            {
                return BadRequestError("Candidate ID cannot be empty.");
            }

            try
            {
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

                var response = await _ragService.QueryAsync(queryBuilder.ToString());

                return Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating interview questions for candidate {CandidateId}",
                    request?.CandidateId);
                return Error($"Error generating interview questions: {ex.Message}");
            }
        }

        [HttpPost("candidates/{candidateId}/match-jobs")]
        public async Task<IActionResult> MatchCandidateWithJobs(string candidateId, [FromQuery] int limit = 10)
        {
            try
            {
                var matchResults = await _jobMatchingService.MatchCandidateWithJobsAsync(candidateId, limit);
                return Success(matchResults);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Candidate not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching jobs for candidate {CandidateId}", candidateId);
                return Error($"Error matching jobs: {ex.Message}");
            }
        }

        [HttpPost("jobs/{jobPostingId}/match-candidates")]
        public async Task<IActionResult> MatchJobWithCandidates(string jobPostingId, [FromQuery] int limit = 10)
        {
            try
            {
                var matchResults = await _jobMatchingService.MatchJobWithCandidatesAsync(jobPostingId, limit);
                return Success(matchResults);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Job posting not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching candidates for job {JobPostingId}", jobPostingId);
                return Error($"Error matching candidates: {ex.Message}");
            }
        }

        [HttpPost("applications/{applicationId}/analyze")]
        public async Task<IActionResult> AnalyzeApplication(string applicationId)
        {
            try
            {
                var analysis = await _jobMatchingService.AnalyzeApplicationAsync(applicationId);
                return Success(analysis);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Application not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing application {ApplicationId}", applicationId);
                return Error($"Error analyzing application: {ex.Message}");
            }
        }
    }
}