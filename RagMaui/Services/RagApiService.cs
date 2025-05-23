using RagMaui.Models;
using RagMaui.Config;
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

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        public async Task<Conversation> CreateConversationAsync(string title)
        {
            try
            {
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(new { Title = title }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/rag/conversations", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Conversation>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
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
                await UpdateAuthenticationHeader();

                var content = new StringContent(
                    JsonSerializer.Serialize(new { Query = query }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"api/rag/conversations/{conversationId}/query", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RagResponse>>(_jsonOptions);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying in conversation: {ex.Message}");
                throw;
            }
        }

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

                var response = await _httpClient.PostAsync("api/rag/documents", form);
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
    }
}