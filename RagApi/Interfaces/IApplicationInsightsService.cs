using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models.Dto;

namespace RagApi.Interfaces;

/// <summary>
/// Service for retrieving telemetry data from Application Insights
/// </summary>
public interface IApplicationInsightsService
{
    /// <summary>
    /// Queries Application Insights for error telemetry within the specified date range
    /// </summary>
    /// <param name="startDate">Start date for the query</param>
    /// <param name="endDate">End date for the query</param>
    /// <param name="severity">Optional severity filter (Error, Critical, Warning)</param>
    /// <param name="skip">Number of records to skip (for pagination)</param>
    /// <param name="take">Number of records to take (for pagination)</param>
    /// <returns>Paginated list of errors with their details</returns>
    Task<ErrorsResponse> GetErrorsAsync(DateTime startDate, DateTime endDate, string? severity = null, int skip = 0, int take = 20);

    /// <summary>
    /// Queries Application Insights for log entries
    /// </summary>
    /// <param name="startDate">Start date for the query</param>
    /// <param name="endDate">End date for the query</param>
    /// <param name="severity">Optional severity filter</param>
    /// <param name="query">Optional text to search for in logs</param>
    /// <param name="skip">Number of records to skip (for pagination)</param>
    /// <param name="take">Number of records to take (for pagination)</param>
    /// <returns>Paginated list of log entries</returns>
    Task<LogsResponse> GetLogsAsync(DateTime startDate, DateTime endDate, string? severity = null, string? query = null, int skip = 0, int take = 20);

    /// <summary>
    /// Queries Application Insights for custom metrics
    /// </summary>
    /// <param name="metricName">Name of the metric to query</param>
    /// <param name="timespan">Timespan to query</param>
    /// <returns>Time series data for the requested metric</returns>
    Task<MetricsResponse> GetMetricsAsync(string metricName, TimeSpan timespan);

    /// <summary>
    /// Retrieves performance data from Application Insights
    /// </summary>
    /// <param name="startDate">Start date for the query</param>
    /// <param name="endDate">End date for the query</param>
    /// <param name="operationNames">Optional list of operation names to filter by</param>
    /// <returns>Performance metrics for operations</returns>
    Task<PerformanceResponse> GetPerformanceDataAsync(DateTime startDate, DateTime endDate, IEnumerable<string>? operationNames = null);
}