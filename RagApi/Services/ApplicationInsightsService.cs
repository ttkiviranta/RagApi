using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RagApi.Interfaces;
using RagApi.Models.Dto;

namespace RagApi.Services;

/// <summary>
/// Service for retrieving telemetry data from Application Insights
/// </summary>
public class ApplicationInsightsService : IApplicationInsightsService
{
    private readonly ILogger<ApplicationInsightsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _appId;
    private readonly string _apiKey;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly bool _isConfigured;

    /// <summary>
    /// Initializes a new instance of the ApplicationInsightsService
    /// </summary>
    public ApplicationInsightsService(
        ILogger<ApplicationInsightsService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("ApplicationInsights");
        
        // Extract Application ID from the connection string
        var connectionString = configuration["ApplicationInsights:ConnectionString"] ?? string.Empty;
        _appId = ExtractApplicationIdFromConnectionString(connectionString);
        
        // Get API key from configuration
        _apiKey = configuration["ApplicationInsights:ApiKey"] ?? string.Empty;
        
        // Check if we have valid configuration
        _isConfigured = !string.IsNullOrEmpty(_appId) && !string.IsNullOrEmpty(_apiKey) && 
                       !_apiKey.Equals("your-api-key-here", StringComparison.OrdinalIgnoreCase);
        
        if (!_isConfigured)
        {
            _logger.LogWarning("Application Insights API is not properly configured. Please check your ApiKey in appsettings.json");
        }
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        // Configure the HTTP client
        _httpClient.BaseAddress = new Uri("https://api.applicationinsights.io/v1/");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
    }

    /// <inheritdoc/>
    public async Task<ErrorsResponse> GetErrorsAsync(DateTime startDate, DateTime endDate, string? severity = null, int skip = 0, int take = 20)
    {
        try
        {
            // Check if the service is properly configured
            if (!_isConfigured)
            {
                _logger.LogError("Application Insights API is not properly configured. Please set a valid API key.");
                throw new InvalidOperationException("Application Insights API is not configured properly. Please check your API key.");
            }
            
            _logger.LogInformation("Querying Application Insights for errors between {StartDate} and {EndDate}", startDate, endDate);
            
            // Build the Kusto query for errors
            var query = new StringBuilder();
            query.AppendLine("exceptions");
            query.AppendLine($"| where timestamp >= datetime({startDate:O}) and timestamp <= datetime({endDate:O})");
            
            // Add severity filter if provided
            if (!string.IsNullOrEmpty(severity))
            {
                query.AppendLine($"| where severityLevel == '{severity}'");
            }
            
            // Get total count with a separate query
            var countQuery = new StringBuilder();
            countQuery.AppendLine("exceptions");
            countQuery.AppendLine($"| where timestamp >= datetime({startDate:O}) and timestamp <= datetime({endDate:O})");
            
            if (!string.IsNullOrEmpty(severity))
            {
                countQuery.AppendLine($"| where severityLevel == '{severity}'");
            }
            
            countQuery.AppendLine("| count");
                
            // Execute the count query
            var countQueryResult = await ExecuteQueryAsync<CountResult>(countQuery.ToString());
            var totalCount = countQueryResult.FirstOrDefault()?.Count ?? 0;
            
            // Continue building the main query - make sure timestamp is explicitly selected
            query.AppendLine("| order by timestamp desc");
            query.AppendLine("| project id = operation_Id,");
            query.AppendLine("          timestamp,");  // This should map to the Timestamp property
            query.AppendLine("          message = innermostMessage,");
            query.AppendLine("          exceptionType = type,");
            query.AppendLine("          stackTrace = details[0].rawStack,");
            query.AppendLine("          operationName = operation_Name,");
            query.AppendLine("          operationId = operation_Id,");
            query.AppendLine("          severity = severityLevel,");
            query.AppendLine("          userId = user_Id,");
            query.AppendLine("          url = customDimensions.url,");
            query.AppendLine("          properties = customDimensions");
            
            // Add pagination - Note: In Kusto, use 'take' not 'limit'
            if (skip > 0)
            {
                query.AppendLine($"| skip {skip}");
            }
            query.AppendLine($"| take {take}");
            
            // Log the final query for debugging
            _logger.LogInformation("Executing query: {Query}", query.ToString());
            
            // Execute the main query
            var queryResult = await ExecuteQueryAsync<ErrorResult>(query.ToString());
            
            // Map to DTOs
            var errors = queryResult.Select(r => new ErrorDto
            {
                Id = r.Id ?? string.Empty,
                Timestamp = r.Timestamp,
                Message = r.Message ?? string.Empty,
                ExceptionType = r.ExceptionType ?? string.Empty,
                StackTrace = r.StackTrace ?? string.Empty,
                OperationName = r.OperationName ?? string.Empty,
                OperationId = r.OperationId ?? string.Empty,
                Severity = r.Severity ?? string.Empty,
                UserId = r.UserId ?? string.Empty,
                Url = r.Url ?? string.Empty,
                Properties = ConvertDynamicToProperties(r.Properties)
            }).ToList();
            
            return new ErrorsResponse
            {
                Errors = errors,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Application Insights for errors: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<LogsResponse> GetLogsAsync(DateTime startDate, DateTime endDate, string? severity = null, string? query = null, int skip = 0, int take = 20)
    {
        try
        {
            // Check if the service is properly configured
            if (!_isConfigured)
            {
                _logger.LogError("Application Insights API is not properly configured. Please set a valid API key.");
                throw new InvalidOperationException("Application Insights API is not configured properly. Please check your API key.");
            }
            
            _logger.LogInformation("Querying Application Insights for logs between {StartDate} and {EndDate}", startDate, endDate);
            
            // Build the Kusto query for logs
            var kustoQuery = new StringBuilder();
            kustoQuery.AppendLine("traces");
            kustoQuery.AppendLine($"| where timestamp >= datetime({startDate:O}) and timestamp <= datetime({endDate:O})");
            
            // Add severity filter if provided
            if (!string.IsNullOrEmpty(severity))
            {
                kustoQuery.AppendLine($"| where severityLevel == '{severity}'");
            }
            
            // Add text filter if provided
            if (!string.IsNullOrEmpty(query))
            {
                kustoQuery.AppendLine($"| where message contains '{query}'");
            }
            
            // Get total count with a separate query
            var countQuery = new StringBuilder();
            countQuery.AppendLine("traces");
            countQuery.AppendLine($"| where timestamp >= datetime({startDate:O}) and timestamp <= datetime({endDate:O})");
            
            // Add same filters to count query
            if (!string.IsNullOrEmpty(severity))
            {
                countQuery.AppendLine($"| where severityLevel == '{severity}'");
            }
            
            if (!string.IsNullOrEmpty(query))
            {
                countQuery.AppendLine($"| where message contains '{query}'");
            }
            
            countQuery.AppendLine("| count");
                
            // Execute the count query
            var countQueryResult = await ExecuteQueryAsync<CountResult>(countQuery.ToString());
            var totalCount = countQueryResult.FirstOrDefault()?.Count ?? 0;
            
            // Continue building the main query
            kustoQuery.AppendLine("| order by timestamp desc");
            kustoQuery.AppendLine("| project timestamp,");
            kustoQuery.AppendLine("          severityLevel,");
            kustoQuery.AppendLine("          message,");
            kustoQuery.AppendLine("          operationName = operation_Name,");
            kustoQuery.AppendLine("          properties = customDimensions");
            
            // Add pagination - Note: In Kusto, use 'take' not 'limit'
            if (skip > 0)
            {
                kustoQuery.AppendLine($"| skip {skip}");
            }
            kustoQuery.AppendLine($"| take {take}");
            
            // Execute the main query
            var queryResult = await ExecuteQueryAsync<LogResult>(kustoQuery.ToString());
            
            // Map to DTOs
            var logs = queryResult.Select(r => new LogEntry
            {
                Timestamp = r.Timestamp,
                SeverityLevel = r.SeverityLevel ?? string.Empty,
                Message = r.Message ?? string.Empty,
                OperationName = r.OperationName ?? string.Empty,
                Properties = ConvertDynamicToProperties(r.Properties)
            }).ToList();
            
            return new LogsResponse
            {
                Logs = logs,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Application Insights for logs: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MetricsResponse> GetMetricsAsync(string metricName, TimeSpan timespan)
    {
        try
        {
            // Check if the service is properly configured
            if (!_isConfigured)
            {
                _logger.LogError("Application Insights API is not properly configured. Please set a valid API key.");
                throw new InvalidOperationException("Application Insights API is not configured properly. Please check your API key.");
            }
            
            _logger.LogInformation("Querying Application Insights for metric {MetricName}", metricName);
            
            // Calculate time range
            var endTime = DateTime.UtcNow;
            var startTime = endTime - timespan;
            
            // Build the Kusto query for metrics
            var query = $@"
                customMetrics
                | where name == '{metricName}'
                | where timestamp >= datetime({startTime:O}) and timestamp <= datetime({endTime:O})
                | project timestamp, value
                | order by timestamp asc
            ";
            
            // Execute the query
            var queryResult = await ExecuteQueryAsync<MetricResult>(query);
            
            // Map to DTOs
            var dataPoints = queryResult.Select(r => new MetricDataPoint
            {
                Timestamp = r.Timestamp,
                Value = r.Value
            }).ToList();
            
            return new MetricsResponse
            {
                MetricName = metricName,
                DataPoints = dataPoints
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Application Insights for metrics: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PerformanceResponse> GetPerformanceDataAsync(DateTime startDate, DateTime endDate, IEnumerable<string>? operationNames = null)
    {
        try
        {
            // Check if the service is properly configured
            if (!_isConfigured)
            {
                _logger.LogError("Application Insights API is not properly configured. Please set a valid API key.");
                throw new InvalidOperationException("Application Insights API is not configured properly. Please check your API key.");
            }
            
            _logger.LogInformation("Querying Application Insights for performance data between {StartDate} and {EndDate}", startDate, endDate);
            
            // Build the Kusto query for performance data
            var query = new StringBuilder();
            query.AppendLine("requests");
            query.AppendLine($"| where timestamp >= datetime({startDate:O}) and timestamp <= datetime({endDate:O})");
            
            // Add operation names filter if provided
            if (operationNames?.Any() == true)
            {
                var operationNamesStr = string.Join("', '", operationNames);
                query.AppendLine($"| where name in ('{operationNamesStr}')");
            }
            
            query.AppendLine("| summarize");
            query.AppendLine("    avgDuration = avg(duration),");
            query.AppendLine("    count = count(),");
            query.AppendLine("    successCount = countif(success == true)");
            query.AppendLine("    by name");
            query.AppendLine("| project");
            query.AppendLine("    operationName = name,");
            query.AppendLine("    averageDurationMs = avgDuration,");
            query.AppendLine("    successRate = 100.0 * successCount / count,");
            query.AppendLine("    count");
            query.AppendLine("| order by avgDuration desc");
            
            // Execute the query
            var queryResult = await ExecuteQueryAsync<PerformanceResult>(query.ToString());
            
            // Map to DTOs
            var metrics = queryResult.Select(r => new PerformanceMetric
            {
                OperationName = r.OperationName ?? string.Empty,
                AverageDurationMs = r.AverageDurationMs,
                SuccessRate = r.SuccessRate,
                Count = r.Count
            }).ToList();
            
            return new PerformanceResponse
            {
                Metrics = metrics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Application Insights for performance data: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    // Helper methods

    /// <summary>
    /// Executes a Kusto query against Application Insights
    /// </summary>
    private async Task<List<T>> ExecuteQueryAsync<T>(string query)
    {
        try
        {
            var requestUri = $"apps/{_appId}/query?query={Uri.EscapeDataString(query)}";
            var response = await _httpClient.GetAsync(requestUri);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Application Insights query failed with status {StatusCode}: {ErrorContent}. Query: {Query}", 
                    response.StatusCode, errorContent, query);
                    
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Unauthorized access to Application Insights API. Check your API key.");
                }
                
                // For BadRequest, include more details about the query issue
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new ArgumentException($"Invalid Kusto query. Status: {response.StatusCode}, Error: {errorContent}, Query: {query}");
                }
                
                throw new Exception($"Application Insights query failed with status {response.StatusCode}. Error: {errorContent}");
            }
            
            var queryResponse = await response.Content.ReadFromJsonAsync<QueryResponse>(_jsonOptions);
            
            if (queryResponse?.Tables == null || queryResponse.Tables.Count == 0 || 
                queryResponse.Tables[0].Rows == null || queryResponse.Tables[0].Rows.Count == 0)
            {
                return new List<T>();
            }
            
            var results = new List<T>();
            var columns = queryResponse.Tables[0].Columns;
            
            // Debug: Log column information to help diagnose the issue
            _logger.LogInformation("Query columns: {Columns}", 
                string.Join(", ", columns.Select(c => $"{c.Name}:{c.Type}")));
            
            foreach (var row in queryResponse.Tables[0].Rows)
            {
                var item = Activator.CreateInstance<T>();
                
                // Debug: Log first row data to see what we're working with
                if (results.Count == 0)
                {
                    _logger.LogInformation("First row data: {RowData}", 
                        string.Join(", ", row.Select((value, index) => $"[{index}] {columns[index].Name}={value?.ToString() ?? "null"}")));
                }
                
                for (var i = 0; i < columns.Count; i++)
                {
                    var columnName = columns[i].Name;
                    var property = typeof(T).GetProperty(columnName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                    if (property != null && i < row.Count)
                    {
                        var value = row[i];
                        
                        if (value != null)
                        {
                            try
                            {
                                // Handle type conversions more carefully
                                if (property.PropertyType == typeof(DateTimeOffset) && value is string dateTimeStr)
                                {
                                    if (DateTimeOffset.TryParse(dateTimeStr, out var dateTimeValue))
                                    {
                                        property.SetValue(item, dateTimeValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(DateTimeOffset) && value is JsonElement jsonDateTime)
                                {
                                    if (jsonDateTime.ValueKind == JsonValueKind.String && 
                                        DateTimeOffset.TryParse(jsonDateTime.GetString(), out var dateTimeValue))
                                    {
                                        property.SetValue(item, dateTimeValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(DateTime) && value is string dateStr)
                                {
                                    if (DateTime.TryParse(dateStr, out var dateValue))
                                    {
                                        property.SetValue(item, dateValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(DateTime) && value is JsonElement jsonDate)
                                {
                                    if (jsonDate.ValueKind == JsonValueKind.String && 
                                        DateTime.TryParse(jsonDate.GetString(), out var dateValue))
                                    {
                                        property.SetValue(item, dateValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(double) && value is string doubleStr)
                                {
                                    if (double.TryParse(doubleStr, out var doubleValue))
                                    {
                                        property.SetValue(item, doubleValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(double) && value is JsonElement jsonDouble)
                                {
                                    if (jsonDouble.TryGetDouble(out var doubleValue))
                                    {
                                        property.SetValue(item, doubleValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(int) && value is string intStr)
                                {
                                    if (int.TryParse(intStr, out var intValue))
                                    {
                                        property.SetValue(item, intValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(int) && value is JsonElement jsonInt)
                                {
                                    if (jsonInt.TryGetInt32(out var intValue))
                                    {
                                        property.SetValue(item, intValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(long) && value is string longStr)
                                {
                                    if (long.TryParse(longStr, out var longValue))
                                    {
                                        property.SetValue(item, longValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(long) && value is JsonElement jsonLong)
                                {
                                    if (jsonLong.TryGetInt64(out var longValue))
                                    {
                                        property.SetValue(item, longValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(bool) && value is string boolStr)
                                {
                                    if (bool.TryParse(boolStr, out var boolValue))
                                    {
                                        property.SetValue(item, boolValue);
                                    }
                                }
                                else if (property.PropertyType == typeof(bool) && value is JsonElement jsonBool)
                                {
                                    if (jsonBool.ValueKind == JsonValueKind.True)
                                    {
                                        property.SetValue(item, true);
                                    }
                                    else if (jsonBool.ValueKind == JsonValueKind.False)
                                    {
                                        property.SetValue(item, false);
                                    }
                                }
                                else if (property.PropertyType == typeof(string))
                                {
                                    // Always convert to string safely
                                    property.SetValue(item, value?.ToString() ?? string.Empty);
                                }
                                else
                                {
                                    // Try generic conversion as fallback
                                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                                    property.SetValue(item, convertedValue);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning("Failed to convert value '{Value}' of type '{ValueType}' to property '{PropertyName}' of type '{PropertyType}': {Error}", 
                                    value, value.GetType().Name, property.Name, property.PropertyType.Name, ex.Message);
                                
                                // As a last resort, try to set as string if the property accepts it
                                if (property.PropertyType == typeof(string))
                                {
                                    property.SetValue(item, value?.ToString() ?? string.Empty);
                                }
                            }
                        }
                    }
                }
                
                results.Add(item);
            }
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Application Insights query: {Query}", query);
            throw;
        }
    }

    /// <summary>
    /// Extracts the Application ID from an Application Insights connection string
    /// </summary>
    private string ExtractApplicationIdFromConnectionString(string connectionString)
    {
        var parts = connectionString.Split(';');
        foreach (var part in parts)
        {
            if (part.StartsWith("ApplicationId=", StringComparison.OrdinalIgnoreCase))
            {
                return part.Substring("ApplicationId=".Length);
            }
        }
        
        _logger.LogWarning("ApplicationId not found in connection string. Application Insights API may not work properly.");
        return string.Empty;
    }

    /// <summary>
    /// Converts dynamic properties to a dictionary
    /// </summary>
    private Dictionary<string, string> ConvertDynamicToProperties(object? properties)
    {
        var result = new Dictionary<string, string>();
        
        if (properties == null)
        {
            return result;
        }
        
        try
        {
            // Try to serialize and deserialize the dynamic object
            var json = JsonSerializer.Serialize(properties);
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
            
            if (dictionary != null)
            {
                foreach (var kvp in dictionary)
                {
                    result[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }
            }
        }
        catch
        {
            // If conversion fails, at least try to get the string representation
            result["value"] = properties.ToString() ?? string.Empty;
        }
        
        return result;
    }

    #region Query Result Classes

    private class QueryResponse
    {
        public List<Table> Tables { get; set; } = new();
    }

    private class Table
    {
        public string Name { get; set; } = string.Empty;
        public List<Column> Columns { get; set; } = new();
        public List<List<object?>> Rows { get; set; } = new();
    }

    private class Column
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    private class ErrorResult
    {
        public string? Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string? Message { get; set; }
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
        public string? OperationName { get; set; }
        public string? OperationId { get; set; }
        public string? Severity { get; set; }
        public string? UserId { get; set; }
        public string? Url { get; set; }
        public object? Properties { get; set; }
    }

    private class LogResult
    {
        public DateTimeOffset Timestamp { get; set; }
        public string? SeverityLevel { get; set; }
        public string? Message { get; set; }
        public string? OperationName { get; set; }
        public object? Properties { get; set; }
    }

    private class MetricResult
    {
        public DateTimeOffset Timestamp { get; set; }
        public double Value { get; set; }
    }

    private class PerformanceResult
    {
        public string? OperationName { get; set; }
        public double AverageDurationMs { get; set; }
        public double SuccessRate { get; set; }
        public int Count { get; set; }
    }

    private class CountResult
    {
        public int Count { get; set; }
    }

    #endregion
}