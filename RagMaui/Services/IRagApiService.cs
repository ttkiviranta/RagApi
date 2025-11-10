using RagMaui.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RagMaui.Services
{
    /// <summary>
    /// Interface for RAG API service
    /// </summary>
    public interface IRagApiService
    {
        // RAG Queries
        Task<RagResponse> QueryAsync(string query);
        Task<RagResponse> QueryInConversationAsync(string conversationId, string query);

        // Conversation Management
        Task<Conversation> CreateConversationAsync(string title);
        Task<Conversation> GetConversationAsync(string conversationId);
        Task<List<Conversation>> GetConversationsAsync();

        // Document Management
        Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string documentType);

        // System Prompts
        Task<List<SystemPrompt>> GetSystemPromptsAsync();
        Task<SystemPrompt> GetSystemPromptAsync(string id);
        Task<SystemPrompt> GetDefaultSystemPromptAsync();
        Task<SystemPrompt> CreateSystemPromptAsync(SystemPrompt prompt);
        Task<SystemPrompt> UpdateSystemPromptAsync(string id, SystemPrompt prompt);
        Task DeleteSystemPromptAsync(string id);

        // Job Posting Management
        Task<List<JobPosting>> GetJobPostingsAsync();
        Task<JobPosting> GetJobPostingAsync(string id);

        // Job Matching
        Task<JobPostingMatchResult> MatchCandidateWithJobsAsync(string candidateId, int limit = 10);
        Task<CandidateMatchResult> MatchJobWithCandidatesAsync(string jobPosting, List<string> candidateIds, int maxCandidates = 10);

        // Application Management
        Task<List<JobApplication>> GetApplicationsAsync();
        Task<JobApplication> GetApplicationAsync(string id);
        Task<JobApplication> CreateApplicationAsync(JobApplication application);
        Task<bool> UpdateApplicationStatusAsync(string id, string status);

        // Interview Management
        Task<List<Interview>> GetInterviewsAsync();
        Task<Interview> GetInterviewAsync(string id);
        Task<List<Interview>> GetInterviewsByApplicationIdAsync(string applicationId);
        Task<Interview> CreateInterviewAsync(Interview interview);
        Task<Interview> UpdateInterviewAsync(string id, Interview interview);
        Task<List<string>> GenerateInterviewQuestionsAsync(string id);

        // Candidate Management
        Task<List<Candidate>> GetCandidatesAsync();
        Task<Candidate> GetCandidateAsync(string id);
        Task<Candidate> CreateCandidateAsync(Candidate candidate);
        Task<Candidate> UpdateCandidateAsync(string id, Candidate candidate);
    }
}