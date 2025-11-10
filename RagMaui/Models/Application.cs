namespace RagMaui.Models
{
    public class JobApplication
    {
        public string Id { get; set; } = string.Empty;
        public string CandidateId { get; set; } = string.Empty;
        public string JobPostingId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? MatchScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigaatiotietoja
        public Candidate? Candidate { get; set; }
        public JobPosting? JobPosting { get; set; }
    }
}
