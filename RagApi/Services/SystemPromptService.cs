using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    public class SystemPromptService : ISystemPromptService
    {
        private readonly ApplicationDbContext _dbContext;

        public SystemPromptService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SystemPrompt> CreateSystemPromptAsync(string name, string description, string promptText, bool isDefault = false)
        {
            // If this is set as default, unset any other default prompt
            if (isDefault)
            {
                var currentDefault = await _dbContext.SystemPrompts.FirstOrDefaultAsync(sp => sp.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            var systemPrompt = new SystemPrompt
            {
                Name = name,
                Description = description,
                PromptText = promptText,
                IsDefault = isDefault
            };

            _dbContext.SystemPrompts.Add(systemPrompt);
            await _dbContext.SaveChangesAsync();

            return systemPrompt;
        }

        public async Task<SystemPrompt> GetSystemPromptAsync(string id)
        {
            return await _dbContext.SystemPrompts
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<List<SystemPrompt>> GetSystemPromptsAsync()
        {
            return await _dbContext.SystemPrompts
                .OrderByDescending(sp => sp.IsDefault)
                .ThenBy(sp => sp.Name)
                .ToListAsync();
        }

        public async Task<SystemPrompt> GetDefaultSystemPromptAsync()
        {
            return await _dbContext.SystemPrompts
                .FirstOrDefaultAsync(sp => sp.IsDefault);
        }

        public async Task<SystemPrompt> UpdateSystemPromptAsync(string id, string name, string description, string promptText, bool isDefault)
        {
            var systemPrompt = await _dbContext.SystemPrompts.FindAsync(id);
            if (systemPrompt == null)
            {
                throw new ArgumentException($"System prompt not found with ID {id}");
            }

            // If this is set as default, unset any other default prompt
            if (isDefault && !systemPrompt.IsDefault)
            {
                var currentDefault = await _dbContext.SystemPrompts.FirstOrDefaultAsync(sp => sp.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            systemPrompt.Name = name;
            systemPrompt.Description = description;
            systemPrompt.PromptText = promptText;
            systemPrompt.IsDefault = isDefault;
            systemPrompt.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return systemPrompt;
        }

        public async Task<bool> DeleteSystemPromptAsync(string id)
        {
            var systemPrompt = await _dbContext.SystemPrompts.FindAsync(id);
            if (systemPrompt == null)
            {
                return false;
            }

            // Check if this is the default prompt
            if (systemPrompt.IsDefault)
            {
                throw new InvalidOperationException("Cannot delete the default system prompt. Set another prompt as default first.");
            }

            // Remove all user associations
            var userPrompts = await _dbContext.UserSystemPrompts.Where(usp => usp.SystemPromptId == id).ToListAsync();
            _dbContext.UserSystemPrompts.RemoveRange(userPrompts);

            _dbContext.SystemPrompts.Remove(systemPrompt);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignSystemPromptToUserAsync(string userId, string systemPromptId, bool isDefault = false)
        {
            // Verify user and system prompt exist
            var user = await _dbContext.Users.FindAsync(userId);
            var systemPrompt = await _dbContext.SystemPrompts.FindAsync(systemPromptId);

            if (user == null || systemPrompt == null)
            {
                return false;
            }

            // If this is set as default for the user, unset any other default prompt for this user
            if (isDefault)
            {
                var currentUserDefault = await _dbContext.UserSystemPrompts
                    .FirstOrDefaultAsync(usp => usp.UserId == userId && usp.IsDefault);

                if (currentUserDefault != null)
                {
                    currentUserDefault.IsDefault = false;
                }
            }

            // Check if assignment already exists
            var existingAssignment = await _dbContext.UserSystemPrompts
                .FirstOrDefaultAsync(usp => usp.UserId == userId && usp.SystemPromptId == systemPromptId);

            if (existingAssignment != null)
            {
                // Update existing assignment
                existingAssignment.IsDefault = isDefault;
            }
            else
            {
                // Create new assignment
                _dbContext.UserSystemPrompts.Add(new UserSystemPrompt
                {
                    UserId = userId,
                    SystemPromptId = systemPromptId,
                    IsDefault = isDefault
                });
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<SystemPrompt> GetUserSystemPromptAsync(string userId)
        {
            // Try to get user's default system prompt
            var userSystemPrompt = await _dbContext.UserSystemPrompts
                .Include(usp => usp.SystemPrompt)
                .FirstOrDefaultAsync(usp => usp.UserId == userId && usp.IsDefault);

            if (userSystemPrompt != null)
            {
                return userSystemPrompt.SystemPrompt;
            }

            // If user has no default, get the global default
            return await GetDefaultSystemPromptAsync();
        }
    }
}
