using RagMaui.Models;
using System.IO;
using System.Threading.Tasks;

namespace RagMaui.Services
{
    /// <summary>
    /// Interface for the RAG API service
    /// </summary>
    public interface IRagApiService
    {
        /// <summary>
        /// Performs a query against the RAG system
        /// </summary>
        Task<RagResponse> QueryAsync(string query);

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        Task<Conversation> CreateConversationAsync(string title);

        /// <summary>
        /// Gets all conversations for the current user
        /// </summary>
        Task<List<Conversation>> GetConversationsAsync();

        /// <summary>
        /// Gets a specific conversation by ID
        /// </summary>
        Task<Conversation> GetConversationAsync(string conversationId);

        /// <summary>
        /// Performs a query within a conversation context
        /// </summary>
        Task<RagResponse> QueryInConversationAsync(string conversationId, string query);

        /// <summary>
        /// Uploads a document for processing
        /// </summary>
        Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string documentType);
    }
}
