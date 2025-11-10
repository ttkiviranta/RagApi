namespace RagApi.Models;

/// <summary>
/// Represents an interview in the recruitment process
/// </summary>
public class Interview
{
    /// <summary>
    /// Unique identifier for the interview
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the application this interview is for
    /// </summary>
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>
    /// When the interview is scheduled to take place
    /// </summary>
    public DateTime ScheduledDate { get; set; }

    /// <summary>
    /// ID of the user conducting the interview
    /// </summary>
    public string InterviewerId { get; set; } = string.Empty; // Viiteavain Users-tauluun

    /// <summary>
    /// Type of the interview (phone, video, in-person)
    /// </summary>
    public string InterviewType { get; set; } = string.Empty; // Phone, Video, In-person

    /// <summary>
    /// Current status of the interview
    /// </summary>
    public string Status { get; set; } = string.Empty; // Scheduled, Completed, Cancelled

    /// <summary>
    /// Notes about the interview
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Questions to be asked during the interview (JSON format)
    /// </summary>
    public string Questions { get; set; } = string.Empty; // JSON-muotoinen lista kysymyksistä

    /// <summary>
    /// Feedback from the interviewer after the interview
    /// </summary>
    public string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// When the interview was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the interview was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to related application
    /// </summary>
    public Application? Application { get; set; }

    /// <summary>
    /// Navigation property to interviewer user
    /// </summary>
    public User? Interviewer { get; set; }

    /// <summary>
    /// Checks if the interview is upcoming
    /// </summary>
    public bool IsUpcoming => Status == "Scheduled" && ScheduledDate > DateTime.UtcNow;
}
