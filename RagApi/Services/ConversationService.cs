using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    public class ConversationService : IConversationService
    {
        private readonly ApplicationDbContext _dbContext;

        public ConversationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Conversation> CreateConversationAsync(string title, string userId = null)
        {
            var conversation = new Conversation
            {
                Title = title
            };

            // Set user ID if provided
            if (!string.IsNullOrEmpty(userId))
            {
                // Check if user exists
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException($"User not found with ID {userId}");
                }

                // Set user ID using shadow property
                _dbContext.Entry(conversation).Property("UserId").CurrentValue = userId;
            }

            _dbContext.Conversations.Add(conversation);
            await _dbContext.SaveChangesAsync();

            return conversation;
        }

        public async Task<Conversation> GetConversationAsync(string conversationId)
        {
            return await _dbContext.Conversations
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }

        public async Task<List<Conversation>> GetConversationsAsync()
        {
            return await _dbContext.Conversations
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(string userId)
        {
            return await _dbContext.Conversations
                .Where(c => EF.Property<string>(c, "UserId") == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Message> AddUserMessageAsync(string conversationId, string content)
        {
            var conversation = await _dbContext.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                throw new ArgumentException($"Conversation not found with ID {conversationId}");
            }

            var message = new Message
            {
                ConversationId = conversationId,
                Type = MessageType.User,
                Content = content,
                SearchResultsJson = "[]"
            };

            conversation.UpdatedAt = DateTime.UtcNow;

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return message;
        }

        public async Task<Message> AddSystemMessageAsync(string conversationId, string content, List<SearchResult> searchResults)
        {
            var conversation = await _dbContext.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                throw new ArgumentException($"Conversation not found with ID {conversationId}");
            }

            var message = new Message
            {
                ConversationId = conversationId,
                Type = MessageType.System,
                Content = content,
                SearchResultsJson = JsonSerializer.Serialize(searchResults)
            };

            conversation.UpdatedAt = DateTime.UtcNow;

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return message;
        }
      
        public async Task<List<Message>> GetConversationMessagesAsync(string conversationId)
        {
            return await _dbContext.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}