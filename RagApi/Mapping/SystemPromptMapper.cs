using Riok.Mapperly.Abstractions;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Mapping
{
    /// <summary>
    /// Compile-time generated mapper for SystemPrompts
    /// </summary>
    [Mapper]
    public sealed partial class SystemPromptMapper
    {
        /// <summary>
        /// Maps SystemPrompt entity to SystemPromptResponseDto
        /// </summary>
        public SystemPromptResponseDto MapToDto(SystemPrompt systemPrompt)
        {
            if (systemPrompt == null) return null;

            // Implementoi itse kokonaan mappauskoodin
            return new SystemPromptResponseDto(
                systemPrompt.Id,
                systemPrompt.Name,
                systemPrompt.Description,
                systemPrompt.PromptText,
                systemPrompt.IsDefault,
                systemPrompt.CreatedAt,
                systemPrompt.UpdatedAt,
                systemPrompt.UserSystemPrompts?.Count ?? 0  // Laske käyttäjien määrä
            );
        }

        /// <summary>
        /// Maps CreateSystemPromptRequest to SystemPrompt entity
        /// </summary>
        public SystemPrompt MapToEntity(CreateSystemPromptRequest request)
        {
            if (request == null) return null;

            return new SystemPrompt
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                PromptText = request.PromptText,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Updates SystemPrompt entity properties from UpdateSystemPromptRequest
        /// </summary>
        public void MapToEntity(SystemPrompt entity, UpdateSystemPromptRequest request)
        {
            if (entity == null || request == null) return;

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.PromptText = request.PromptText;
            entity.IsDefault = request.IsDefault;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}