using System.Text.Json;
using RagApi.Interfaces.Agents;

namespace RagApi.Models.Agents;

/// <summary>
/// Base interface for all agents in the system
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Unique identifier for the agent
    /// </summary>
  string Name { get; }
    
    /// <summary>
    /// Agent capabilities and what it can do
    /// </summary>
    AgentCapability[] Capabilities { get; }
    
    /// <summary>
    /// Execute the agent's primary function
    /// </summary>
    Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context information passed to agents
/// </summary>
public record AgentContext
{
    public string UserId { get; init; } = string.Empty;
public Dictionary<string, object> Parameters { get; init; } = [];
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result returned by agent execution
/// </summary>
public record AgentResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object> Data { get; init; } = [];
    public List<string> Errors { get; init; } = [];
    public TimeSpan ExecutionTime { get; init; }
    public string AgentName { get; init; } = string.Empty;
}

/// <summary>
/// Agent capability definitions
/// </summary>
public record AgentCapability(string Name, string Description, AgentCapabilityType Type);

/// <summary>
/// Types of agent capabilities
/// </summary>
public enum AgentCapabilityType
{
    DocumentProcessing,
    DataAnalysis,
    Matching,
    Communication,
    Workflow,
    Reporting
}

/// <summary>
/// Workflow definition for orchestrating multiple agents
/// </summary>
public record WorkflowDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<WorkflowStep> Steps { get; init; } = [];
    public Dictionary<string, object> DefaultParameters { get; init; } = [];
}

/// <summary>
/// Individual step in a workflow
/// </summary>
public record WorkflowStep
{
    public string Id { get; init; } = string.Empty;
    public string AgentName { get; init; } = string.Empty;
    public Dictionary<string, object> Parameters { get; init; } = [];
    public List<string> DependsOn { get; init; } = [];
  public bool ContinueOnError { get; init; } = false;
}

/// <summary>
/// Result of workflow execution
/// </summary>
public record WorkflowResult
{
    public string WorkflowName { get; init; } = string.Empty;
    public bool Success { get; init; }
    public List<AgentResult> AgentResults { get; init; } = [];
    public TimeSpan TotalExecutionTime { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
}