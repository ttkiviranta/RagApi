using RagMaui.Config;
using RagMaui.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RagMaui.Services
{
    /// <summary>
    /// Implementation of the RAG API service
    /// </summary>
    public class RagApiService : IRagApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IAuthenticationService _authService;

        /// <summary>
        /// Initializes a new instance of the RagApiService
        /// </summary>
        public RagApiService(IAuthenticationService authService)
        {
            _authService = authService;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(AuthConfig.ApiBaseUrl);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Subscribe to authentication state changes
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        /// <summary>
        /// Handler for authentication state changes
        /// </summary>
        private void OnAuthenticationStateChanged(bool isAuthenticated)
        {
            // Update the authentication header when auth state changes
            UpdateAuthenticationHeader();
        }

        /// <summary>
        /// Updates the authentication header with the current token
        /// </summary>
        private async Task UpdateAuthenticationHeader()
        {
            if (_authService.IsAuthenticated)
            {
                string token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        #region Conversation Management

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        public async Task<Conversation> CreateConversationAsync(string title)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"RagApiService: Creating conversation '{title}' to {_httpClient.BaseAddress}");
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(new { Title = title }),
                    Encoding.UTF8,
                    "application/json");

                System.Diagnostics.Debug.WriteLine($"RagApiService: Sending POST to api/rag/conversations");
                var response = await _httpClient.PostAsync("api/rag/conversations", content);
                System.Diagnostics.Debug.WriteLine($"RagApiService: Response status: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Conversation>>(_jsonOptions);
                System.Diagnostics.Debug.WriteLine($"RagApiService: Conversation created successfully: {result?.Data?.Id}");
                return result.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RagApiService: Error creating conversation: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Error creating conversation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific conversation by ID
        /// </summary>
        public async Task<Conversation> GetConversationAsync(string conversationId)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync($"api/rag/conversations/{conversationId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Conversation>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting conversation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all conversations for the current user
        /// </summary>
        public async Task<List<Conversation>> GetConversationsAsync()
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync("api/rag/conversations");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Conversation>>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting conversations: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region RAG Queries

        /// <summary>
        /// Performs a query against the RAG system
        /// </summary>
        public async Task<RagResponse> QueryAsync(string query)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(new { Query = query }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/rag/query", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RagResponse>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Performs a query within a conversation context
        /// </summary>
        public async Task<RagResponse> QueryInConversationAsync(string conversationId, string query)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"RagApiService: Querying conversation {conversationId} with: '{query}'");
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(new { Query = query }),
                    Encoding.UTF8,
                    "application/json");

                System.Diagnostics.Debug.WriteLine($"RagApiService: Sending POST to api/rag/conversations/{conversationId}/query");
                var response = await _httpClient.PostAsync($"api/rag/conversations/{conversationId}/query", content);
                System.Diagnostics.Debug.WriteLine($"RagApiService: Query response status: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RagResponse>>(_jsonOptions);
                System.Diagnostics.Debug.WriteLine($"RagApiService: Query successful, answer length: {result?.Data?.Answer?.Length ?? 0}");
                return result.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RagApiService: Error querying conversation: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Error querying in conversation: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Document Management

        /// <summary>
        /// Uploads a document for processing
        /// </summary>
        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string documentType)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var form = new MultipartFormDataContent();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                form.Add(fileContent, "file", fileName);
                form.Add(new StringContent(documentType), "DocumentType");

                var response = await _httpClient.PostAsync("api/documents", form);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<UploadDocumentResponse>>(_jsonOptions);
                return result.Data.DocumentId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading document: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region System Prompts

        /// <summary>
        /// Gets all system prompts
        /// </summary>
        public async Task<List<SystemPrompt>> GetSystemPromptsAsync()
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync("api/systemprompt");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<SystemPrompt>>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting system prompts: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific system prompt by ID
        /// </summary>
        public async Task<SystemPrompt> GetSystemPromptAsync(string id)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync($"api/systemprompt/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SystemPrompt>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting system prompt: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the default system prompt
        /// </summary>
        public async Task<SystemPrompt> GetDefaultSystemPromptAsync()
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync("api/systemprompt/default");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SystemPrompt>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting default system prompt: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a new system prompt
        /// </summary>
        public async Task<SystemPrompt> CreateSystemPromptAsync(SystemPrompt prompt)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(prompt),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/systemprompt", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SystemPrompt>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating system prompt: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing system prompt
        /// </summary>
        public async Task<SystemPrompt> UpdateSystemPromptAsync(string id, SystemPrompt prompt)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(prompt),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/systemprompt/{id}", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SystemPrompt>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating system prompt: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deletes a system prompt
        /// </summary>
        public async Task DeleteSystemPromptAsync(string id)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.DeleteAsync($"api/systemprompt/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting system prompt: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Job Posting Management

        /// <summary>
        /// Gets all job postings
        /// </summary>
        public async Task<List<JobPosting>> GetJobPostingsAsync()
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync("api/jobpostings");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<JobPosting>>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting job postings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific job posting by ID
        /// </summary>
        public async Task<JobPosting> GetJobPostingAsync(string id)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync($"api/jobpostings/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<JobPosting>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting job posting: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Matching

        /// <summary>
        /// Match a candidate with available job postings
        /// </summary>
        public async Task<JobPostingMatchResult> MatchCandidateWithJobsAsync(string candidateId, int limit = 10)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.PostAsync($"api/job-matching/candidates/{candidateId}/match-jobs?limit={limit}", null);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<JobPostingMatchResult>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error matching candidate with jobs: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Match job postings with candidate resumes
        /// </summary>
        public async Task<CandidateMatchResult> MatchJobWithCandidatesAsync(string jobPosting, List<string> candidateIds, int maxCandidates = 10)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(new { jobPosting, candidateIds, maxCandidates }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/rag/job-matching", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<CandidateMatchResult>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error matching job with candidates: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Application Management

        /// <summary>
        /// Gets all applications
        /// </summary>
        public async Task<List<JobApplication>> GetApplicationsAsync()
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync("api/applications");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<JobApplication>>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting applications: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific application by ID
        /// </summary>
        public async Task<JobApplication> GetApplicationAsync(string id)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync($"api/applications/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<JobApplication>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting application: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a new application
        /// </summary>
        public async Task<JobApplication> CreateApplicationAsync(JobApplication application)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(application),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/applications", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<JobApplication>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating application: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates application status
        /// </summary>
        public async Task<bool> UpdateApplicationStatusAsync(string id, string status)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(new { status }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/applications/{id}/status", content);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating application status: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Interview Management

        /// <summary>
        /// Gets all interviews
        /// </summary>
        public async Task<List<Interview>> GetInterviewsAsync()
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync("api/interviews");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Interview>>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting interviews: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific interview by ID
        /// </summary>
        public async Task<Interview> GetInterviewAsync(string id)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync($"api/interviews/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Interview>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting interview: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all interviews for an application
        /// </summary>
        public async Task<List<Interview>> GetInterviewsByApplicationIdAsync(string applicationId)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync($"api/interviews/application/{applicationId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Interview>>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting interviews by application ID: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a new interview
        /// </summary>
        public async Task<Interview> CreateInterviewAsync(Interview interview)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(interview),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/interviews", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Interview>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating interview: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates an interview
        /// </summary>
        public async Task<Interview> UpdateInterviewAsync(string id, Interview interview)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(interview),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/interviews/{id}", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Interview>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating interview: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generates interview questions
        /// </summary>
        public async Task<List<string>> GenerateInterviewQuestionsAsync(string id)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.PostAsync($"api/interviews/{id}/questions", null);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<string>>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating interview questions: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Candidate Management

        /// <summary>
        /// Gets all candidates
        /// </summary>
        public async Task<List<Candidate>> GetCandidatesAsync()
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync("api/candidates");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Candidate>>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting candidates: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific candidate by ID
        /// </summary>
        public async Task<Candidate> GetCandidateAsync(string id)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var response = await _httpClient.GetAsync($"api/candidates/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Candidate>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting candidate: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a new candidate
        /// </summary>
        public async Task<Candidate> CreateCandidateAsync(Candidate candidate)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(candidate),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/candidates", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Candidate>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating candidate: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates a candidate
        /// </summary>
        public async Task<Candidate> UpdateCandidateAsync(string id, Candidate candidate)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(candidate),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/candidates/{id}", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Candidate>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating candidate: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}