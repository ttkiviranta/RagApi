using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Main service for RAG (Retrieval Augmented Generation) functionality
    /// </summary>
    public interface IRagService
    {
        /// <summary>
        /// Processes a PDF file for RAG
        /// </summary>
        /// <param name="pdfStream">The PDF file stream</param>
        /// <param name="fileName">Original filename</param>
        /// <returns>Document identifier</returns>
        Task<string> ProcessPdfAsync(Stream pdfStream, string fileName);

        /// <summary>
        /// Performs a single query without conversation context
        /// </summary>
        /// <param name="query">User query</param>
        /// <param name="userId">Optional user ID for personalized responses</param>
        /// <returns>RAG response with answer and sources</returns>
        Task<RagResponse> QueryAsync(string query, string userId = null);

        /// <summary>
        /// Performs a query within a conversation context
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="query">User query</param>
        /// <param name="userId">Optional user ID for personalized responses</param>
        /// <returns>RAG response with answer and sources</returns>
        Task<RagResponse> QueryWithConversationAsync(string conversationId, string query, string userId = null);

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        /// <param name="title">Conversation title</param>
        /// <param name="userId">Optional user ID to associate with the conversation</param>
        /// <returns>Created conversation</returns>
        Task<Conversation> CreateConversationAsync(string title, string userId = null);

        /// <summary>
        /// Gets all conversations
        /// </summary>
        /// <param name="userId">Optional user ID to filter conversations</param>
        /// <returns>List of all conversations</returns>
        Task<List<Conversation>> GetConversationsAsync(string userId = null);

        /// <summary>
        /// Retrieves a conversation by ID
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <returns>The conversation or null if not found</returns>
        Task<Conversation> GetConversationAsync(string conversationId);
    }
}