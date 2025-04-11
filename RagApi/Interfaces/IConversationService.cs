using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service for managing conversations and messages
    /// </summary>
    public interface IConversationService
    {
        /// <summary>
        /// Creates a new conversation
        /// </summary>
        /// <param name="title">Conversation title</param>
        /// <param name="userId">Optional user ID to associate with the conversation</param>
        /// <returns>Created conversation</returns>
        Task<Conversation> CreateConversationAsync(string title, string userId = null);

        /// <summary>
        /// Retrieves a conversation by ID
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <returns>The conversation or null if not found</returns>
        Task<Conversation> GetConversationAsync(string conversationId);

        /// <summary>
        /// Gets all conversations
        /// </summary>
        /// <returns>List of all conversations</returns>
        Task<List<Conversation>> GetConversationsAsync();

        /// <summary>
        /// Gets all conversations for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's conversations</returns>
        Task<List<Conversation>> GetUserConversationsAsync(string userId);

        /// <summary>
        /// Adds a user message to a conversation
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="content">Message content</param>
        /// <returns>Created message</returns>
        Task<Message> AddUserMessageAsync(string conversationId, string content);

        /// <summary>
        /// Adds a system message to a conversation
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="content">Message content</param>
        /// <param name="searchResults">Related search results</param>
        /// <returns>Created message</returns>
        Task<Message> AddSystemMessageAsync(string conversationId, string content, List<SearchResult> searchResults);

        /// <summary>
        /// Gets all messages for a conversation
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <returns>List of messages in chronological order</returns>
        Task<List<Message>> GetConversationMessagesAsync(string conversationId);
    }
}