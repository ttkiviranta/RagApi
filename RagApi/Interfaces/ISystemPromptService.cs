using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service for managing system prompts
    /// </summary>
    public interface ISystemPromptService
    {
        /// <summary>
        /// Creates a new system prompt
        /// </summary>
        /// <param name="name">Name of the prompt</param>
        /// <param name="description">Description of the prompt</param>
        /// <param name="promptText">The actual prompt text</param>
        /// <param name="isDefault">Whether this is the default prompt</param>
        /// <returns>Created system prompt</returns>
        Task<SystemPrompt> CreateSystemPromptAsync(string name, string description, string promptText, bool isDefault = false);

        /// <summary>
        /// Gets a system prompt by ID
        /// </summary>
        /// <param name="id">System prompt ID</param>
        /// <returns>The system prompt or null if not found</returns>
        Task<SystemPrompt> GetSystemPromptAsync(string id);

        /// <summary>
        /// Gets all system prompts
        /// </summary>
        /// <returns>List of all system prompts</returns>
        Task<List<SystemPrompt>> GetSystemPromptsAsync();

        /// <summary>
        /// Gets the default system prompt
        /// </summary>
        /// <returns>The default system prompt</returns>
        Task<SystemPrompt> GetDefaultSystemPromptAsync();

        /// <summary>
        /// Updates an existing system prompt
        /// </summary>
        /// <param name="id">System prompt ID</param>
        /// <param name="name">Updated name</param>
        /// <param name="description">Updated description</param>
        /// <param name="promptText">Updated prompt text</param>
        /// <param name="isDefault">Updated default status</param>
        /// <returns>Updated system prompt</returns>
        Task<SystemPrompt> UpdateSystemPromptAsync(string id, string name, string description, string promptText, bool isDefault);

        /// <summary>
        /// Deletes a system prompt
        /// </summary>
        /// <param name="id">System prompt ID</param>
        /// <returns>True if successfully deleted</returns>
        Task<bool> DeleteSystemPromptAsync(string id);

        /// <summary>
        /// Assigns a system prompt to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="systemPromptId">System prompt ID</param>
        /// <param name="isDefault">Whether this is the user's default prompt</param>
        /// <returns>True if assignment was successful</returns>
        Task<bool> AssignSystemPromptToUserAsync(string userId, string systemPromptId, bool isDefault = false);

        /// <summary>
        /// Gets the system prompt for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's default system prompt or global default if none assigned</returns>
        Task<SystemPrompt> GetUserSystemPromptAsync(string userId);
    }
}
