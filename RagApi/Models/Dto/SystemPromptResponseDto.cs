using System;
using System.Collections.Generic;

namespace RagApi.Models.Dto
{
    /// <summary>
    /// DTO for returning system prompt information
    /// </summary>
    public record SystemPromptResponseDto(
        string Id,
        string Name,
        string Description,
        string PromptText,
        bool IsDefault,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        int UserCount);
}
