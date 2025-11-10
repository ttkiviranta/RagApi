namespace RagMaui.Models
{
    public class Interview
    {
        public string Id { get; set; } = string.Empty;
        public string ApplicationId { get; set; } = string.Empty;
        public string InterviewerId { get; set; } = string.Empty;
        public string InterviewerName { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string InterviewType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? Feedback { get; set; }
        public decimal? Score { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigaatiotietoja
        public Application? Application { get; set; }
    }
}
