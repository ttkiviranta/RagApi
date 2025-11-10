using System.Collections.Generic;

namespace RagMaui.Models
{
    public class JobPostingMatchResult
    {
        public Candidate Candidate { get; set; } = new Candidate();
        public List<JobMatchItem> Matches { get; set; } = new List<JobMatchItem>();
    }

    public class JobMatchItem
    {
        public JobPosting JobPosting { get; set; } = new JobPosting();
        public decimal MatchScore { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }

    public class CandidateMatchResult
    {
        public JobPosting JobPosting { get; set; } = new JobPosting();
        public List<CandidateMatchItem> Matches { get; set; } = new List<CandidateMatchItem>();
    }

    public class CandidateMatchItem
    {
        public Candidate Candidate { get; set; } = new Candidate();
        public decimal MatchScore { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}
