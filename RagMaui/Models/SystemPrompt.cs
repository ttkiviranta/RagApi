namespace RagMaui.Models
{
    public class SystemPrompt
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public string? CreatedByUserName { get; set; }
    }
}
