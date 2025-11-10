using System.ComponentModel.DataAnnotations;

namespace RagApi.Models.Dto
{
    public record CreateSystemPromptRequest(
        [Required]
        [StringLength(100)] string Name,
        [StringLength(500)] string Description,
        [Required] string PromptText,
        bool IsDefault);

    public record UpdateSystemPromptRequest(
        [Required]
        [StringLength(100)] string Name,
        [StringLength(500)] string Description,
        [Required] string PromptText,
        bool IsDefault);

    public record AssignSystemPromptRequest(bool IsDefault = true);
}

