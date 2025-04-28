using RagApi.Models.Dto;
using System.ComponentModel.DataAnnotations;

public class InterviewCreateDto
{
    [Required]
    public string ApplicationId { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }

    [Required]
    public string InterviewType { get; set; }

    public string Notes { get; set; }
}

public class InterviewUpdateDto
{
    public DateTime ScheduledDate { get; set; }

    public string InterviewType { get; set; }

    public string Status { get; set; }

    public string Notes { get; set; }

    public string Feedback { get; set; }
}

public class InterviewResponseDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int InterviewerId { get; set; }
    public string InterviewType { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
    public List<string> Questions { get; set; }
    public string Feedback { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigaatiokohteiden tiedot
    public ApplicationResponseDto Application { get; set; }
    public UserResponseDto Interviewer { get; set; }
}
