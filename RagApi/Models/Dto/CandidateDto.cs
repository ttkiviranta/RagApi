using System;
using System.ComponentModel.DataAnnotations;

public record CandidateCreateDto
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

    public CandidateCreateDto(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
    
    // Parametriton konstruktori
    public CandidateCreateDto()
    {
    }
}

public record CandidateUpdateDto : CandidateCreateDto
{
    public CandidateUpdateDto(string firstName, string lastName, string email)
        : base(firstName, lastName, email)
    {
    }
    
    // Parametriton konstruktori
    public CandidateUpdateDto() : base()
    {
    }
}

public record CandidateResponseDto
{
    public string Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
    public string LinkedInProfile { get; init; }
    public string CurrentPosition { get; init; }
    public string CurrentCompany { get; init; }
    public string ResumeDocumentId { get; init; }
    public string CoverLetterDocumentId { get; init; }
    public string Skills { get; init; }
    public string Location { get; init; }
    public DateTime CreatedAt { get; init; }
}
