using Microsoft.Extensions.Logging;
using RagApi.Interfaces.Agents;
using RagApi.Models.Agents;
using System.Diagnostics;

namespace RagApi.Services.Agents;

/// <summary>
/// Main orchestrator for managing and executing agent workflows
/// </summary>
public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly Dictionary<string, IAgent> _agents = [];
    private readonly Dictionary<string, WorkflowDefinition> _workflows = [];
    private readonly ILogger<AgentOrchestrator> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AgentOrchestrator(ILogger<AgentOrchestrator> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        InitializeDefaultWorkflows();
    }

    public void RegisterAgent(IAgent agent)
    {
        _agents[agent.Name] = agent;
      _logger.LogInformation("Registered agent: {AgentName} with {CapabilityCount} capabilities", 
      agent.Name, agent.Capabilities.Length);
    }

    public async Task<AgentResult> ExecuteAgentAsync(string agentName, AgentContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
   if (!_agents.TryGetValue(agentName, out var agent))
            {
       return new AgentResult
              {
             Success = false,
    Message = $"Agent '{agentName}' not found",
         Errors = [$"Agent '{agentName}' is not registered"],
         ExecutionTime = stopwatch.Elapsed,
        AgentName = agentName
       };
     }

      _logger.LogInformation("Executing agent {AgentName} with correlation ID {CorrelationId}", 
        agentName, context.CorrelationId);

     var result = await agent.ExecuteAsync(context, cancellationToken);
         
_logger.LogInformation("Agent {AgentName} completed in {ExecutionTime}ms with success: {Success}", 
          agentName, stopwatch.ElapsedMilliseconds, result.Success);

          return result with { ExecutionTime = stopwatch.Elapsed, AgentName = agentName };
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Error executing agent {AgentName}", agentName);
  
        return new AgentResult
            {
    Success = false,
      Message = $"Agent execution failed: {ex.Message}",
          Errors = [ex.Message],
    ExecutionTime = stopwatch.Elapsed,
          AgentName = agentName
    };
}
}

    public async Task<WorkflowResult> ExecuteWorkflowAsync(string workflowName, AgentContext context, CancellationToken cancellationToken = default)
    {
        if (!_workflows.TryGetValue(workflowName, out var workflow))
        {
            return new WorkflowResult
 {
       WorkflowName = workflowName,
    Success = false,
  CorrelationId = context.CorrelationId,
 AgentResults = [new AgentResult { 
       Success = false, 
          Message = $"Workflow '{workflowName}' not found",
           Errors = [$"Workflow '{workflowName}' is not registered"]
         }]
  };
        }

        return await ExecuteWorkflowAsync(workflow, context, cancellationToken);
    }

    public async Task<WorkflowResult> ExecuteWorkflowAsync(WorkflowDefinition workflow, AgentContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var agentResults = new List<AgentResult>();
        var executedSteps = new HashSet<string>();
        var workflowData = new Dictionary<string, object>(context.Parameters);

        _logger.LogInformation("Starting workflow {WorkflowName} with {StepCount} steps", 
          workflow.Name, workflow.Steps.Count);

        try
        {
            // Execute steps in dependency order
            while (executedSteps.Count < workflow.Steps.Count)
      {
        var readySteps = workflow.Steps
       .Where(step => !executedSteps.Contains(step.Id))
            .Where(step => step.DependsOn.All(dep => executedSteps.Contains(dep)))
            .ToList();

        if (!readySteps.Any())
             {
             var remainingSteps = workflow.Steps.Where(step => !executedSteps.Contains(step.Id)).ToList();
   _logger.LogError("Circular dependency detected in workflow {WorkflowName}. Remaining steps: {Steps}", 
  workflow.Name, string.Join(", ", remainingSteps.Select(s => s.Id)));
 break;
         }

     // Execute ready steps in parallel
      var stepTasks = readySteps.Select(async step =>
             {
            var stepContext = context with 
        { 
        Parameters = new Dictionary<string, object>(workflowData.Concat(step.Parameters))
      };

          var result = await ExecuteAgentAsync(step.AgentName, stepContext, cancellationToken);
   
     // Merge successful results back into workflow data
        if (result.Success)
{
      foreach (var kvp in result.Data)
        {
       workflowData[kvp.Key] = kvp.Value;
       }
     }
    else if (!step.ContinueOnError)
     {
    _logger.LogWarning("Step {StepId} failed and ContinueOnError is false. Workflow may be affected.", step.Id);
  }

           lock (executedSteps)
      {
      executedSteps.Add(step.Id);
        }

    return result;
                });

          var stepResults = await Task.WhenAll(stepTasks);
         agentResults.AddRange(stepResults);

  // Check if any critical step failed
     if (stepResults.Any(r => !r.Success && !readySteps.First(s => s.AgentName == r.AgentName).ContinueOnError))
      {
                 _logger.LogWarning("Critical step failed in workflow {WorkflowName}", workflow.Name);
     }
    }

         var success = agentResults.All(r => r.Success) || 
              agentResults.Where(r => !r.Success).All(r => 
         workflow.Steps.Any(s => s.AgentName == r.AgentName && s.ContinueOnError));

        _logger.LogInformation("Workflow {WorkflowName} completed in {ExecutionTime}ms with success: {Success}", 
     workflow.Name, stopwatch.ElapsedMilliseconds, success);

 return new WorkflowResult
        {
           WorkflowName = workflow.Name,
        Success = success,
                AgentResults = agentResults,
  TotalExecutionTime = stopwatch.Elapsed,
   CorrelationId = context.CorrelationId
      };
  }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {WorkflowName}", workflow.Name);
        
     return new WorkflowResult
   {
   WorkflowName = workflow.Name,
      Success = false,
      AgentResults = agentResults,
      TotalExecutionTime = stopwatch.Elapsed,
              CorrelationId = context.CorrelationId
            };
        }
    }

    public IEnumerable<IAgent> GetRegisteredAgents() => _agents.Values;

    public IEnumerable<WorkflowDefinition> GetAvailableWorkflows() => _workflows.Values;

    private void InitializeDefaultWorkflows()
    {
        // CV Processing Workflow
        _workflows["cv-processing"] = new WorkflowDefinition
        {
     Name = "cv-processing",
            Description = "Complete CV processing and candidate creation workflow",
            Steps = [
      new WorkflowStep
                {
  Id = "extract-cv",
     AgentName = "document-analysis-agent",
         Parameters = new Dictionary<string, object> { ["documentType"] = "CV" }
 },
      new WorkflowStep
   {
            Id = "create-candidate",
     AgentName = "candidate-creation-agent",
        DependsOn = ["extract-cv"]
           },
                new WorkflowStep
     {
         Id = "match-jobs",
      AgentName = "job-matching-agent", 
 DependsOn = ["create-candidate"],
    ContinueOnError = true
        }
            ]
   };

  // Job Posting Workflow
     _workflows["job-posting-processing"] = new WorkflowDefinition
        {
Name = "job-posting-processing",
 Description = "Job posting analysis and processing workflow",
 Steps = [
          new WorkflowStep
  {
          Id = "analyze-job-posting",
                  AgentName = "document-analysis-agent",
     Parameters = new Dictionary<string, object> { ["documentType"] = "JobPosting" }
   },
    new WorkflowStep
      {
            Id = "match-candidates",
     AgentName = "job-matching-agent",
           DependsOn = ["analyze-job-posting"],
    ContinueOnError = true
   }
      ]
      };

        _logger.LogInformation("Initialized {WorkflowCount} default workflows", _workflows.Count);
    }
}