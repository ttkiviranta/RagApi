using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using RagApi.Interfaces;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers
{
    /// <summary>
    /// Controller for accessing Application Insights telemetry data
    /// </summary>
    [ApiController]
    [Route("api/insights")]
    // [Authorize(Roles = "Admin")] // Restrict access to admin users
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    public class InsightsController : BaseController
    {
        private readonly IApplicationInsightsService _appInsightsService;
        private readonly ILogger<InsightsController> _logger;

        /// <summary>
        /// Initializes a new instance of the InsightsController
        /// </summary>
        public InsightsController(
            IApplicationInsightsService appInsightsService,
            IRequestContext requestContext,
            ILogger<InsightsController> logger) 
            : base(requestContext)
        {
            _appInsightsService = appInsightsService;
            _logger = logger;
        }

        /// <summary>
        /// Gets error telemetry from Application Insights within a specified date range
        /// </summary>
        /// <param name="startDate">Start date for the query (defaults to 2 hours ago)</param>
        /// <param name="endDate">End date for the query (defaults to current time)</param>
        /// <param name="severity">Optional severity filter (Error, Critical, Warning)</param>
        /// <param name="skip">Number of records to skip (for pagination)</param>
        /// <param name="take">Number of records to take (for pagination)</param>
        /// <returns>Paginated list of errors with their details</returns>
        [HttpGet("errors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetErrors(
            DateTime? startDate = null,
            DateTime? endDate = null,
            [FromQuery(Name = "severity")] string? severity = null,
            [FromQuery(Name = "skip")][Range(0, int.MaxValue)] int skip = 0,
            [FromQuery(Name = "take")][Range(1, 100)] int take = 20)
        {
            try
            {
                // Set default values if not provided
                endDate ??= DateTime.UtcNow;
                startDate ??= endDate.Value.AddHours(-2); // Default to 2 hours ago

                // Validate input
                if (startDate > endDate)
                {
                    return BadRequestError("Start date must be before end date");
                }

                // Ensure endDate isn't in the future
                if (endDate > DateTime.UtcNow)
                {
                    endDate = DateTime.UtcNow;
                }

                // Validate severity parameter if provided
                if (!string.IsNullOrEmpty(severity) && 
                    !new[] { "Error", "Critical", "Warning" }.Contains(severity, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequestError("Invalid severity value. Valid values are: Error, Critical, Warning");
                }

                // Cap the time range to 30 days to avoid performance issues
                var maxTimeSpan = TimeSpan.FromDays(30);
                if (endDate.Value - startDate.Value > maxTimeSpan)
                {
                    startDate = endDate.Value - maxTimeSpan;
                    _logger.LogWarning("Time range was limited to 30 days");
                }

                _logger.LogInformation(
                    "Getting errors from {StartDate} to {EndDate} with severity {Severity}, skip {Skip}, take {Take}",
                    startDate, endDate, severity ?? "Any", skip, take);

                var result = await _appInsightsService.GetErrorsAsync(startDate.Value, endDate.Value, severity, skip, take);
                
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Application Insights error data");
                return Error("Failed to retrieve error data from Application Insights");
            }
        }

        /// <summary>
        /// Gets log entries from Application Insights within a specified date range
        /// </summary>
        /// <param name="startDate">Start date for the query (defaults to 2 hours ago)</param>
        /// <param name="endDate">End date for the query (defaults to current time)</param>
        /// <param name="severity">Optional severity filter (Verbose, Information, Warning, Error, Critical)</param>
        /// <param name="query">Optional text to search for in logs</param>
        /// <param name="skip">Number of records to skip (for pagination)</param>
        /// <param name="take">Number of records to take (for pagination)</param>
        /// <returns>Paginated list of log entries</returns>
        [HttpGet("logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLogs(
            DateTime? startDate = null,
            DateTime? endDate = null,
            [FromQuery(Name = "severity")] string? severity = null,
            [FromQuery(Name = "query")] string? query = null,
            [FromQuery(Name = "skip")][Range(0, int.MaxValue)] int skip = 0,
            [FromQuery(Name = "take")][Range(1, 100)] int take = 20)
        {
            try
            {
                // Set default values if not provided
                endDate ??= DateTime.UtcNow;
                startDate ??= endDate.Value.AddHours(-2); // Default to 2 hours ago

                // Validate input
                if (startDate > endDate)
                {
                    return BadRequestError("Start date must be before end date");
                }

                // Ensure endDate isn't in the future
                if (endDate > DateTime.UtcNow)
                {
                    endDate = DateTime.UtcNow;
                }

                // Cap the time range to 7 days to avoid performance issues with log queries
                var maxTimeSpan = TimeSpan.FromDays(7);
                if (endDate.Value - startDate.Value > maxTimeSpan)
                {
                    startDate = endDate.Value - maxTimeSpan;
                    _logger.LogWarning("Time range was limited to 7 days for log queries");
                }

                _logger.LogInformation(
                    "Getting logs from {StartDate} to {EndDate} with severity {Severity}, query {Query}, skip {Skip}, take {Take}",
                    startDate, endDate, severity ?? "Any", query ?? "None", skip, take);

                var result = await _appInsightsService.GetLogsAsync(startDate.Value, endDate.Value, severity, query, skip, take);
                
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Application Insights logs");
                return Error("Failed to retrieve logs from Application Insights");
            }
        }

        /// <summary>
        /// Gets custom metrics from Application Insights
        /// </summary>
        /// <param name="metricName">Name of the metric to query</param>
        /// <param name="hours">Number of hours to look back (default: 24)</param>
        /// <returns>Time series data for the requested metric</returns>
        [HttpGet("metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMetrics(
            [Required][FromQuery(Name = "metricName")] string metricName,
            [FromQuery(Name = "hours")][Range(1, 168)] int hours = 24)
        {
            try
            {
                if (string.IsNullOrEmpty(metricName))
                {
                    return BadRequestError("Metric name is required");
                }

                _logger.LogInformation(
                    "Getting metrics for {MetricName} over the past {Hours} hours",
                    metricName, hours);

                var result = await _appInsightsService.GetMetricsAsync(metricName, TimeSpan.FromHours(hours));
                
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Application Insights metrics for {MetricName}", metricName);
                return Error($"Failed to retrieve metrics from Application Insights for {metricName}");
            }
        }

        /// <summary>
        /// Gets performance data from Application Insights
        /// </summary>
        /// <param name="startDate">Start date for the query (defaults to 24 hours ago)</param>
        /// <param name="endDate">End date for the query (defaults to current time)</param>
        /// <param name="operationNames">Optional comma-separated list of operation names to filter by</param>
        /// <returns>Performance metrics for operations</returns>
        [HttpGet("performance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPerformanceData(
            DateTime? startDate = null,
            DateTime? endDate = null,
            [FromQuery(Name = "operationNames")] string? operationNames = null)
        {
            try
            {
                // Set default values if not provided
                endDate ??= DateTime.UtcNow;
                startDate ??= endDate.Value.AddHours(-24); // Default to 24 hours ago for performance metrics

                // Validate input
                if (startDate > endDate)
                {
                    return BadRequestError("Start date must be before end date");
                }

                // Ensure endDate isn't in the future
                if (endDate > DateTime.UtcNow)
                {
                    endDate = DateTime.UtcNow;
                }

                // Cap the time range to 14 days to avoid performance issues
                var maxTimeSpan = TimeSpan.FromDays(14);
                if (endDate.Value - startDate.Value > maxTimeSpan)
                {
                    startDate = endDate.Value - maxTimeSpan;
                    _logger.LogWarning("Time range was limited to 14 days for performance queries");
                }

                // Parse operation names if provided
                IEnumerable<string>? operationsList = null;
                if (!string.IsNullOrEmpty(operationNames))
                {
                    operationsList = operationNames.Split(',').Select(n => n.Trim());
                }

                _logger.LogInformation(
                    "Getting performance data from {StartDate} to {EndDate} for operations: {OperationNames}",
                    startDate, endDate, operationNames ?? "All");

                var result = await _appInsightsService.GetPerformanceDataAsync(startDate.Value, endDate.Value, operationsList);
                
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Application Insights performance data");
                return Error("Failed to retrieve performance data from Application Insights");
            }
        }

        /// <summary>
        /// Simple Hello World method
        /// </summary>
        /// <returns>A greeting message</returns>
        [HttpGet("hello")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HelloWorld()
        {
            var message = "Hello, World! Welcome to the Insights API.";
            _logger.LogInformation("Hello World method called");
            return Success(new { message, timestamp = DateTime.UtcNow });
        }
    }
}