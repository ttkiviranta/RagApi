using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    public class RagService : IRagService
    {
        private readonly IPdfService _pdfService;
        private readonly IVectorSearchService _vectorSearchService;
        private readonly IOpenAIService _openAIService;
        private readonly IConversationService _conversationService;

        public RagService(
            IPdfService pdfService,
            IVectorSearchService vectorSearchService,
            IOpenAIService openAIService,
            IConversationService conversationService)
        {
            _pdfService = pdfService;
            _vectorSearchService = vectorSearchService;
            _openAIService = openAIService;
            _conversationService = conversationService;
        }

        public async Task<string> ProcessPdfAsync(Stream pdfStream, string fileName)
        {
            // Upload PDF to Azure Blob Storage
            string blobName = await _pdfService.UploadPdfAsync(pdfStream, fileName);

            // Extract text from PDF file
            var documentChunks = await _pdfService.ExtractTextFromPdfAsync(blobName);

            // Index text chunks for vector search
            await _vectorSearchService.IndexDocumentChunksAsync(documentChunks);

            return blobName;
        }

        public async Task<RagResponse> QueryAsync(string query, string userId = null)
        {
            try
            {
                // Try to retrieve results with vector search
                var searchResults = await _vectorSearchService.SearchAsync(query);

                // Generate response using ChatGPT
                string answer = await _openAIService.GenerateAnswerAsync(query, searchResults, userId);

                return new RagResponse
                {
                    Answer = answer,
                    SourceResults = searchResults
                };
            }
            catch (Exception ex) when (ex.Message.Contains("not found") || ex.Message.Contains("index"))
            {
                // If index is not found, use direct response without context
                var messages = new List<Message>(); // Empty history
                string answer = await _openAIService.GenerateDirectAnswerWithHistoryAsync(query, messages, userId);

                return new RagResponse
                {
                    Answer = answer,
                    SourceResults = new List<SearchResult>()
                };
            }
        }

        public async Task<RagResponse> QueryWithConversationAsync(string conversationId, string query, string userId = null)
        {
            // Check if conversation exists
            var conversation = await _conversationService.GetConversationAsync(conversationId);
            if (conversation == null)
            {
                throw new ArgumentException($"Conversation not found with ID {conversationId}");
            }

            try
            {
                // Add user message to conversation
                await _conversationService.AddUserMessageAsync(conversationId, query);

                // Get conversation messages
                var messages = await _conversationService.GetConversationMessagesAsync(conversationId);

                List<SearchResult> searchResults = new List<SearchResult>();
                string answer;

                try
                {
                    // Try to retrieve relevant content with vector search
                    searchResults = await _vectorSearchService.SearchAsync(query);

                    // Check if this is the first question in the conversation
                    bool isFirstQuestion = messages.Count <= 1; // Only the user message we just added

                    if (isFirstQuestion)
                    {
                        // For the first question, use the regular answer generation without history
                        // This ensures a clean start without empty context confusion
                        answer = await _openAIService.GenerateAnswerAsync(query, searchResults, userId);
                    }
                    else
                    {
                        // For follow-up questions, use conversation history
                        answer = await _openAIService.GenerateAnswerWithHistoryAsync(query, searchResults, messages, userId);
                    }
                }
                catch (Exception ex) when (ex.Message.Contains("not found") || ex.Message.Contains("index"))
                {
                    // If search fails (e.g., no index), fall back to direct conversation
                    bool isFirstQuestion = messages.Count <= 1;

                    if (isFirstQuestion)
                    {
                        // For first question with no search results, use direct generation without history
                        answer = await _openAIService.GetChatCompletionsAsync(
                            "You are a helpful assistant.",
                            query);
                    }
                    else
                    {
                        // For follow-up questions, use history
                        answer = await _openAIService.GenerateDirectAnswerWithHistoryAsync(query, messages, userId);
                    }
                }

                // Add response to conversation
                await _conversationService.AddSystemMessageAsync(conversationId, answer, searchResults);

                return new RagResponse
                {
                    Answer = answer,
                    SourceResults = searchResults
                };
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in QueryWithConversationAsync: {ex}");
                throw;
            }
        }


        public async Task<Conversation> CreateConversationAsync(string title, string userId = null)
        {
            return await _conversationService.CreateConversationAsync(title, userId);
        }

        public async Task<List<Conversation>> GetConversationsAsync(string userId = null)
        {
            if (userId != null)
            {
                return await _conversationService.GetUserConversationsAsync(userId);
            }

            return await _conversationService.GetConversationsAsync();
        }

        public async Task<Conversation> GetConversationAsync(string conversationId)
        {
            return await _conversationService.GetConversationAsync(conversationId);
        }
    }
}