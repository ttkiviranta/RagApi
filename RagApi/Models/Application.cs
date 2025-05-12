public class Application
{
    public string Id { get; set; }  // Jos pääavain on myös string-tyyppinen
    public string CandidateId { get; set; }  // Muutettu int -> string
    public string JobPostingId { get; set; }  // Oletan että JobPosting.Id on myös string
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; }
    public decimal? MatchScore { get; set; }
    public string MatchAnalysis { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Candidate Candidate { get; set; }
    public JobPosting JobPosting { get; set; }
    public ICollection<Interview> Interviews { get; set; }
}