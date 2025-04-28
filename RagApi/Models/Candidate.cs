using RagApi.Models;

public class Candidate
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string LinkedInProfile { get; set; }
    public string CurrentPosition { get; set; }
    public string CurrentCompany { get; set; }
    public string ResumeDocumentId { get; set; }
    public string CoverLetterDocumentId { get; set; }
    public string Skills { get; set; } // JSON-muotoinen taitojen lista
    public string Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UserId { get; set; } // Viiteavain Users-tauluun

    // Navigointiproperties
    public User User { get; set; }
    public ICollection<Application> Applications { get; set; }
}
