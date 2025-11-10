using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RagApi.Models.Dto;

public record InterviewCreateDto(
    [Required] string ApplicationId,
    [Required] DateTime ScheduledDate,
    [Required] string InterviewType,
    string Notes);

public record InterviewUpdateDto(
    DateTime ScheduledDate,
    string InterviewType,
    string Status,
    string Notes,
    string Feedback);

public record InterviewResponseDto(
    int Id,
    int ApplicationId,
    DateTime ScheduledDate,
    int InterviewerId,
    string InterviewType,
    string Status,
    string Notes,
    List<string> Questions,
    string Feedback,
    DateTime CreatedAt,
    ApplicationResponseDto Application,
    UserResponseDto Interviewer);
