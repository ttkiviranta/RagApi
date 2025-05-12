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
    /// Implementation of job matching service interface
    /// </summary>
    public class JobMatchingService : IJobMatchingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IVectorSearchService _searchService;
        private readonly IOpenAIService _openAIService;

        public JobMatchingService(
            ApplicationDbContext context,
            IBlobStorageService blobStorageService,
            IVectorSearchService searchService,
            IOpenAIService openAIService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _searchService = searchService;
            _openAIService = openAIService;
        }

        /// <inheritdoc/>
        public async Task<JobMatchResult> AnalyzeJobMatchAsync(string resumeBlobPath, string jobPostingBlobPath)
        {
            // Get documents from blob storage
            var resumeContent = await _blobStorageService.DownloadFileAsTextAsync(resumeBlobPath);
            var jobPostingContent = await _blobStorageService.DownloadFileAsTextAsync(jobPostingBlobPath);

            // Prepare prompt template
            var systemPrompt = @"
            You are an expert HR recruiter tasked with analyzing how well a candidate's resume matches a job posting.
            Analyze the resume and job posting carefully to determine the match percentage and provide detailed analysis.
            
            Focus on these key areas:
            1. Skills match
            2. Experience relevance
            3. Education requirements
            4. Technical qualifications
            5. Soft skills alignment
            
            Provide a score between 0 and 100 indicating the overall match percentage.
            Then provide a detailed analysis with specific strengths and potential gaps.
            
            Format your response as a JSON object with the following structure:
            {
                ""score"": 85,
                ""analysis"": ""Detailed analysis text here...""
            }
            ";

            var userPrompt = $@"
            # Resume
            {resumeContent}
            
            # Job Posting
            {jobPostingContent}
            
            Analyze how well this resume matches the job posting. Provide a match score and detailed analysis.
            ";

            // Send request to OpenAI
            var response = await _openAIService.GetChatCompletionsAsync(systemPrompt, userPrompt);

            // Parse response
            try
            {
                var result = JsonSerializer.Deserialize<JobMatchResult>(response);
                return result;
            }
            catch
            {
                // If parsing fails, return default values
                return new JobMatchResult
                {
                    Score = 0,
                    Analysis = "Error analyzing job match. Could not parse the response."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GenerateInterviewQuestionsAsync(string resumeBlobPath, string jobPostingBlobPath)
        {
            // Get documents from blob storage
            var resumeContent = await _blobStorageService.DownloadFileAsTextAsync(resumeBlobPath);
            var jobPostingContent = await _blobStorageService.DownloadFileAsTextAsync(jobPostingBlobPath);

            // Prepare prompt template
            var systemPrompt = @"
            You are an expert HR recruiter tasked with creating tailored interview questions 
            based on a candidate's resume and a job posting.
            
            Generate 5-10 interview questions that:
            1. Probe specific experiences or skills mentioned in the resume that are relevant to the job
            2. Assess how the candidate's background aligns with key job requirements
            3. Include behavioral questions related to the role's challenges
            4. Test technical knowledge required for the position
            5. Explore the candidate's potential cultural fit
            
            Format your response as a JSON array of strings, each containing one interview question.
            ";

            var userPrompt = $@"
            # Resume
            {resumeContent}
            
            # Job Posting
            {jobPostingContent}
            
            Generate tailored interview questions for this candidate and job position.
            ";

            // Send request to OpenAI
            var response = await _openAIService.GetChatCompletionsAsync(systemPrompt, userPrompt);

            // Parse response
            try
            {
                var questions = JsonSerializer.Deserialize<List<string>>(response);
                return questions;
            }
            catch
            {
                // If parsing fails, return default questions
                return new List<string>
                {
                    "Tell me about your relevant experience for this position.",
                    "Why are you interested in this role?",
                    "What are your strengths and weaknesses?",
                    "Describe a challenging project you've worked on.",
                    "Do you have any questions about the position?"
               };
            }
        }

        /// <inheritdoc/>
        public async Task<string> AnalyzeApplicationAsync(string applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new KeyNotFoundException("Application not found");

            // Get documents
            var candidateDocuments = await _context.Documents
                .Where(d => d.EntityId == application.CandidateId && d.DocumentType == "Resume")
                .ToListAsync();

            var jobDocuments = await _context.Documents
                .Where(d => d.EntityId == application.JobPostingId && d.DocumentType == "JobPosting")
                .ToListAsync();

            if (!candidateDocuments.Any() || !jobDocuments.Any())
                throw new InvalidOperationException("Required documents not found");

            // Get document contents
            var resumeContent = await _blobStorageService.DownloadFileAsTextAsync(candidateDocuments.First().BlobStoragePath);
            var jobPostingContent = await _blobStorageService.DownloadFileAsTextAsync(jobDocuments.First().BlobStoragePath);

            // Get search results by querying for information related to candidate and job
            var searchResults = await _searchService.SearchDocumentsAsync(
                $"{application.Candidate.FirstName} {application.Candidate.LastName} {application.JobPosting.Title}",
                5);

            // Prepare prompt template
            var systemPrompt = @"
           You are an expert HR recruiter tasked with analyzing a job application.
           Provide a comprehensive analysis of the candidate's suitability for the position based on their resume and the job posting.
           
           Your analysis should include:
           1. Overall assessment of fit
           2. Key strengths relevant to the position
           3. Potential concerns or missing qualifications
           4. Recommended next steps (interview, reject, further assessment)
           5. Suggested interview focus areas if the candidate is moved forward
           
           Provide your analysis in a well-structured format with clear headings and bullet points where appropriate.
           ";

            var userPrompt = $@"
           # Resume
           {resumeContent}
           
           # Job Posting
           {jobPostingContent}
           
           # Additional Information
           Candidate: {application.Candidate.FirstName} {application.Candidate.LastName}
           Current position: {application.Candidate.CurrentPosition}
           Current company: {application.Candidate.CurrentCompany}
           Skills: {application.Candidate.Skills}
           
           # Search Results
           {string.Join("\n\n", searchResults.Select(r => r.Content))}
           
           Provide a comprehensive analysis of this application.
           ";

            // Send request to OpenAI
            return await _openAIService.GetChatCompletionsAsync(systemPrompt, userPrompt);
        }

        /// <inheritdoc/>
        public async Task<List<JobMatchResultDto>> MatchCandidateWithJobsAsync(string candidateId, int limit = 10)
        {
            var candidate = await _context.Candidates.FindAsync(candidateId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            // Get active job postings
            var jobPostings = await _context.JobPostings
                .Where(jp => jp.Status == "Active" && jp.ExpirationDate >= DateTime.UtcNow)
                .ToListAsync();

            // Get candidate resume
            var candidateDocuments = await _context.Documents
                .Where(d => d.EntityId == candidateId && d.DocumentType == "Resume")
                .ToListAsync();

            if (!candidateDocuments.Any())
                throw new InvalidOperationException("Candidate resume not found");

            var resumeDocument = candidateDocuments.First();
            var matchResults = new List<JobMatchResultDto>();

            // For each job posting, analyze match with candidate
            foreach (var jobPosting in jobPostings)
            {
                var jobDocuments = await _context.Documents
                    .Where(d => d.EntityId == jobPosting.Id && d.DocumentType == "JobPosting")
                    .ToListAsync();

                if (!jobDocuments.Any())
                    continue;

                var jobDocument = jobDocuments.First();

                // Analyze match
                var matchResult = await AnalyzeJobMatchAsync(
                    resumeDocument.BlobStoragePath,
                    jobDocument.BlobStoragePath);

                matchResults.Add(new JobMatchResultDto
                {
                    JobPostingId = jobPosting.Id,
                    JobTitle = jobPosting.Title,
                    MatchScore = matchResult.Score,
                    MatchAnalysis = matchResult.Analysis
                });
            }

            // Return top matches
            return matchResults
                .OrderByDescending(r => r.MatchScore)
                .Take(limit)
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<List<JobMatchResultDto>> MatchJobWithCandidatesAsync(string jobPostingId, int limit = 10)
        {
            var jobPosting = await _context.JobPostings.FindAsync(jobPostingId);
            if (jobPosting == null)
                throw new KeyNotFoundException("Job posting not found");

            // Get all candidates
            var candidates = await _context.Candidates.ToListAsync();

            // Get job posting document
            var jobDocuments = await _context.Documents
                .Where(d => d.EntityId == jobPostingId && d.DocumentType == "JobPosting")
                .ToListAsync();

            if (!jobDocuments.Any())
                throw new InvalidOperationException("Job posting document not found");

            var jobDocument = jobDocuments.First();
            var matchResults = new List<JobMatchResultDto>();

            // For each candidate, analyze match with job posting
            foreach (var candidate in candidates)
            {
                var candidateDocuments = await _context.Documents
                    .Where(d => d.EntityId == candidate.Id && d.DocumentType == "Resume")
                    .ToListAsync();

                if (!candidateDocuments.Any())
                    continue;

                var resumeDocument = candidateDocuments.First();

                // Analyze match
                var matchResult = await AnalyzeJobMatchAsync(
                    resumeDocument.BlobStoragePath,
                    jobDocument.BlobStoragePath);

                matchResults.Add(new JobMatchResultDto
                {
                    JobPostingId = jobPosting.Id,
                    JobTitle = $"{candidate.FirstName} {candidate.LastName}", // Use this field for candidate name
                    MatchScore = matchResult.Score,
                    MatchAnalysis = matchResult.Analysis
                });
            }

            // Return top matches
            return matchResults
                .OrderByDescending(r => r.MatchScore)
                .Take(limit)
                .ToList();
        }
    }
}