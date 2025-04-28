using RagApi.Models;

public class Interview
{
    public string Id { get; set; }
    public string ApplicationId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string InterviewerId { get; set; } // Viiteavain Users-tauluun
    public string InterviewType { get; set; } // Phone, Video, In-person
    public string Status { get; set; } // Scheduled, Completed, Cancelled
    public string Notes { get; set; }
    public string Questions { get; set; } // JSON-muotoinen lista kysymyksistä
    public string Feedback { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigointiproperties
    public Application Application { get; set; }
    public User Interviewer { get; set; }
}
