using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service for generating answers using OpenAI
    /// </summary>
    public interface IOpenAIService
    {
        /// <summary>
        /// Generates an answer based on a query and search results
        /// </summary>
        /// <param name="query">User query</param>
        /// <param name="searchResults">Relevant search results</param>
        /// <param name="userId">Optional user ID to get personalized system prompt</param>
        /// <returns>Generated answer</returns>
        Task<string> GenerateAnswerAsync(string query, List<SearchResult> searchResults, string userId = null);

        /// <summary>
        /// Generates an answer based on a query, search results, and conversation history
        /// </summary>
        /// <param name="query">User query</param>
        /// <param name="searchResults">Relevant search results</param>
        /// <param name="conversationHistory">Previous messages in the conversation</param>
        /// <param name="userId">Optional user ID to get personalized system prompt</param>
        /// <returns>Generated answer</returns>
        Task<string> GenerateAnswerWithHistoryAsync(string query, List<SearchResult> searchResults, List<Message> conversationHistory, string userId = null);

        /// <summary>
        /// Generates a direct answer based on a query and conversation history without document context
        /// </summary>
        /// <param name="query">User query</param>
        /// <param name="conversationHistory">Previous messages in the conversation</param>
        /// <param name="userId">Optional user ID to get personalized system prompt</param>
        /// <returns>Generated answer</returns>
        Task<string> GenerateDirectAnswerWithHistoryAsync(string query, List<Message> conversationHistory, string userId = null);
    }
}