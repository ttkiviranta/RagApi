using System.ComponentModel.DataAnnotations;

public class CandidateCreateDto
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string LinkedInProfile { get; set; } = string.Empty;

    public string CurrentPosition { get; set; } = string.Empty;

    public string CurrentCompany { get; set; } = string.Empty;

    public string Skills { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}


public class CandidateUpdateDto : CandidateCreateDto
{
}

public class CandidateResponseDto
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
    public string Skills { get; set; }
    public string Location { get; set; }
    public DateTime CreatedAt { get; set; }
}
