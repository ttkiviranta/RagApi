using System.ComponentModel.DataAnnotations;

namespace RagApi.Models.Dto
{
    public class CreateSystemPromptRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string PromptText { get; set; }

        public bool IsDefault { get; set; }
    }

    public class UpdateSystemPromptRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string PromptText { get; set; }

        public bool IsDefault { get; set; }
    }

    public class AssignSystemPromptRequest
    {
        public bool IsDefault { get; set; } = true;
    }
}

