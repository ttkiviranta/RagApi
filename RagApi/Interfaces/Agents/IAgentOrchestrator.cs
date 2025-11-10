using RagApi.Models.Agents;

namespace RagApi.Interfaces.Agents;

/// <summary>
/// Agent orchestrator for managing and executing agent workflows
/// </summary>
public interface IAgentOrchestrator
{
    /// <summary>
    /// Register an agent with the orchestrator
    /// </summary>
    void RegisterAgent(IAgent agent);
    
    /// <summary>
    /// Execute a single agent
    /// </summary>
    Task<AgentResult> ExecuteAgentAsync(string agentName, AgentContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute a predefined workflow
    /// </summary>
    Task<WorkflowResult> ExecuteWorkflowAsync(string workflowName, AgentContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute a custom workflow definition
    /// </summary>
Task<WorkflowResult> ExecuteWorkflowAsync(WorkflowDefinition workflow, AgentContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all registered agents
  /// </summary>
 IEnumerable<IAgent> GetRegisteredAgents();
    
    /// <summary>
    /// Get available workflows
    /// </summary>
 IEnumerable<WorkflowDefinition> GetAvailableWorkflows();
}