using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RagApi.Models.Dto;

/// <summary>
/// Represents error telemetry data from Application Insights
/// </summary>
public record ErrorDto
{
    /// <summary>
    /// Unique identifier for the error
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }
    
    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Type of the exception
    /// </summary>
    public string ExceptionType { get; init; } = string.Empty;
    
    /// <summary>
    /// Error stack trace
    /// </summary>
    public string StackTrace { get; init; } = string.Empty;
    
    /// <summary>
    /// Name of the operation where the error occurred
    /// </summary>
    public string OperationName { get; init; } = string.Empty;
    
    /// <summary>
    /// ID of the operation
    /// </summary>
    public string OperationId { get; init; } = string.Empty;
    
    /// <summary>
    /// Error severity (Error, Critical, Warning)
    /// </summary>
    public string Severity { get; init; } = string.Empty;
    
    /// <summary>
    /// User who experienced the error (if available)
    /// </summary>
    public string UserId { get; init; } = string.Empty;
    
    /// <summary>
    /// URL or endpoint where the error occurred
    /// </summary>
    public string Url { get; init; } = string.Empty;
    
    /// <summary>
    /// Additional custom properties
    /// </summary>
    public Dictionary<string, string> Properties { get; init; } = new();
}

/// <summary>
/// Request parameters for querying Application Insights errors
/// </summary>
public record ErrorQueryRequest
{
    /// <summary>
    /// Start date for the query
    /// </summary>
    [Required]
    public DateTime StartDate { get; init; }
    
    /// <summary>
    /// End date for the query
    /// </summary>
    [Required]
    public DateTime EndDate { get; init; }
    
    /// <summary>
    /// Optional severity filter (Error, Critical, Warning)
    /// </summary>
    public string? Severity { get; init; }
    
    /// <summary>
    /// Number of records to skip (for pagination)
    /// </summary>
    public int Skip { get; init; } = 0;
    
    /// <summary>
    /// Number of records to take (for pagination)
    /// </summary>
    public int Take { get; init; } = 20;
}

/// <summary>
/// Paginated response for Application Insights errors
/// </summary>
public record ErrorsResponse
{
    /// <summary>
    /// List of errors
    /// </summary>
    public List<ErrorDto> Errors { get; init; } = new();
    
    /// <summary>
    /// Total count of errors matching the query
    /// </summary>
    public int TotalCount { get; init; }
}

/// <summary>
/// Response model for Application Insights logs
/// </summary>
public record LogsResponse
{
    /// <summary>
    /// List of log entries
    /// </summary>
    public List<LogEntry> Logs { get; init; } = new();
    
    /// <summary>
    /// Total count of logs matching the query
    /// </summary>
    public int TotalCount { get; init; }
}

/// <summary>
/// Represents a single log entry from Application Insights
/// </summary>
public record LogEntry
{
    /// <summary>
    /// Timestamp of the log entry
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }
    
    /// <summary>
    /// Log severity level
    /// </summary>
    public string SeverityLevel { get; init; } = string.Empty;
    
    /// <summary>
    /// Log message
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Operation name
    /// </summary>
    public string OperationName { get; init; } = string.Empty;
    
    /// <summary>
    /// Additional custom properties
    /// </summary>
    public Dictionary<string, string> Properties { get; init; } = new();
}

/// <summary>
/// Response model for Application Insights metrics
/// </summary>
public record MetricsResponse
{
    /// <summary>
    /// Name of the metric
    /// </summary>
    public string MetricName { get; init; } = string.Empty;
    
    /// <summary>
    /// Time series data points
    /// </summary>
    public List<MetricDataPoint> DataPoints { get; init; } = new();
}

/// <summary>
/// Represents a single metric data point
/// </summary>
public record MetricDataPoint
{
    /// <summary>
    /// Timestamp of the data point
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }
    
    /// <summary>
    /// Value of the metric
    /// </summary>
    public double Value { get; init; }
}

/// <summary>
/// Response model for Application Insights performance data
/// </summary>
public record PerformanceResponse
{
    /// <summary>
    /// List of performance metrics
    /// </summary>
    public List<PerformanceMetric> Metrics { get; init; } = new();
}

/// <summary>
/// Represents a single performance metric
/// </summary>
public record PerformanceMetric
{
    /// <summary>
    /// Name of the operation
    /// </summary>
    public string OperationName { get; init; } = string.Empty;
    
    /// <summary>
    /// Average duration in milliseconds
    /// </summary>
    public double AverageDurationMs { get; init; }
    
    /// <summary>
    /// Success rate (0-100)
    /// </summary>
    public double SuccessRate { get; init; }
    
    /// <summary>
    /// Count of operations
    /// </summary>
    public int Count { get; init; }
}