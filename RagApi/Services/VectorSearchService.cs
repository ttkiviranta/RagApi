using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    public class VectorSearchService : IVectorSearchService
    {
        private readonly SearchClient _searchClient;
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _embeddingDeployment;

        public VectorSearchService(
            SearchClient searchClient,
            IConfiguration configuration)
        {
            _searchClient = searchClient;
            _httpClient = new HttpClient();
            _endpoint = configuration["Azure:OpenAI:Endpoint"];
            _apiKey = configuration["Azure:OpenAI:Key"];
            _embeddingDeployment = configuration["Azure:OpenAI:EmbeddingDeployment"];

            // Set API key for all requests
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }

        // Existing method - kept for backward compatibility
        public async Task IndexDocumentChunksAsync(List<DocumentChunk> chunks)
        {
            // Create documents for indexing
            var indexDocuments = new List<SearchDocument>();

            foreach (var chunk in chunks)
            {
                // Get embedding using Azure OpenAI API
                float[] embedding = await GetEmbeddingAsync(chunk.Content);

                // Create search index document
                var searchDoc = new SearchDocument();
                searchDoc["id"] = chunk.Id;
                searchDoc["content"] = chunk.Content;
                searchDoc["source"] = chunk.Source;
                searchDoc["pageNumber"] = chunk.PageNumber;
                searchDoc["contentVector"] = embedding;

                indexDocuments.Add(searchDoc);
            }

            // Send documents for indexing
            var batch = IndexDocumentsBatch.Upload(indexDocuments);
            var indexResponse = await _searchClient.IndexDocumentsAsync(batch);
        }

        // Existing method - kept for backward compatibility
        public async Task<List<SearchResult>> SearchAsync(string query, int maxResults = 5)
        {
            // Get embedding for query
            float[] queryEmbedding = await GetEmbeddingAsync(query);

            // Define vector search options
            var searchOptions = new SearchOptions
            {
                VectorSearch = new VectorSearchOptions
                {
                    Queries = { new VectorizedQuery(queryEmbedding) { KNearestNeighborsCount = maxResults, Fields = { "contentVector" } } }
                },
                Size = maxResults,
                Select = { "id", "content", "source", "pageNumber" }
            };

            // Execute search
            var searchResponse = await _searchClient.SearchAsync<SearchDocument>(null, searchOptions);

            // Transform search results
            var results = new List<SearchResult>();

            await foreach (var result in searchResponse.Value.GetResultsAsync())
            {
                results.Add(new SearchResult
                {
                    Id = result.Document["id"].ToString(),
                    Content = result.Document["content"].ToString(),
                    Source = result.Document["source"].ToString(),
                    Score = result.Score ?? 0
                });
            }

            return results;
        }

        private async Task<float[]> GetEmbeddingAsync(string text)
        {
            // Create embedding request
            var requestData = new
            {
                input = text
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            // Send request to Azure OpenAI API
            var response = await _httpClient.PostAsync(
                $"{_endpoint}openai/deployments/{_embeddingDeployment}/embeddings?api-version=2023-05-15",
                requestContent);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);

            return responseObject.data[0].embedding;
        }

        // Simple response class for JSON deserialization
        private class EmbeddingResponse
        {
            public EmbeddingData[] data { get; set; }

            public class EmbeddingData
            {
                public float[] embedding { get; set; }
            }
        }

        // NEW METHODS IMPLEMENTING UPDATED INTERFACE

        /// <inheritdoc/>
        public async Task IndexDocumentAsync(string documentId, string documentType, string content, string entityId)
        {
            // Get embedding using Azure OpenAI API
            float[] embedding = await GetEmbeddingAsync(content);

            // Create search index document
            var searchDoc = new SearchDocument();
            searchDoc["id"] = documentId;
            searchDoc["type"] = documentType;
            searchDoc["content"] = content;
            searchDoc["entityId"] = entityId;
            searchDoc["createdAt"] = DateTime.UtcNow;
            searchDoc["contentVector"] = embedding;

            // Send document for indexing
            var batch = IndexDocumentsBatch.Upload(new[] { searchDoc });
            await _searchClient.IndexDocumentsAsync(batch);
        }

        /// <inheritdoc/>
        public async Task<List<SearchResult>> SearchDocumentsAsync(string query, int maxResults = 5)
        {
            // Reuse existing search functionality
            return await SearchAsync(query, maxResults);
        }

        /// <inheritdoc/>
        public async Task DeleteDocumentAsync(string documentId)
        {
            // Create a search document with the ID to delete
            var searchDoc = new SearchDocument();
            searchDoc["id"] = documentId;

            // Create a batch with a delete action
            var batch = IndexDocumentsBatch.Delete(new[] { searchDoc });

            try
            {
                // Delete document from search index
                await _searchClient.IndexDocumentsAsync(batch);
            }
            catch (Exception ex)
            {
                // Log the error
                throw new Exception($"Failed to delete document: {ex.Message}", ex);
            }
        }
    }
}