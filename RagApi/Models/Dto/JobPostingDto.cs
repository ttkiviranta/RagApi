using System;
using System.ComponentModel.DataAnnotations;

public record JobPostingCreateDto(
    [Required] string Title,
    [Required] string Description,
    string Requirements,
    string Location,
    string Department,
    string EmploymentType,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string SalaryCurrency,
    DateTime PublishedDate,
    DateTime ExpirationDate,
    [Required] string Status,
    string? JobPostingDocumentId);

public record JobPostingUpdateDto : JobPostingCreateDto
{
    public JobPostingUpdateDto(
        string Title,
        string Description,
        string Requirements,
        string Location,
        string Department,
        string EmploymentType,
        decimal? SalaryMin,
        decimal? SalaryMax,
        string SalaryCurrency,
        DateTime PublishedDate,
        DateTime ExpirationDate,
        string Status,
        string? JobPostingDocumentId)
        : base(Title, Description, Requirements, Location, Department, EmploymentType, SalaryMin, SalaryMax, SalaryCurrency, PublishedDate, ExpirationDate, Status, JobPostingDocumentId)
    {
    }
}

public record JobPostingResponseDto(
    string Id,
    string Title,
    string Description,
    string Requirements,
    string Location,
    string Department,
    string EmploymentType,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string SalaryCurrency,
    string? JobPostingDocumentId,
    DateTime PublishedDate,
    DateTime ExpirationDate,
    string Status,
    DateTime CreatedAt);

