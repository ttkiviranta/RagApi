using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RagApi.Interfaces;
using RagApi.Interfaces.Agents;
using RagApi.Models.Agents;
using RagApi.Models.Dto;

namespace RagApi.Api.Controllers;

/// <summary>
/// Controller for managing and executing AI agents and workflows
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IAgentOrchestrator _agentOrchestrator;
 private readonly IRequestContext _requestContext;

    public AgentsController(IAgentOrchestrator agentOrchestrator, IRequestContext requestContext)
    {
        _agentOrchestrator = agentOrchestrator;
        _requestContext = requestContext;
 }

    /// <summary>
    /// Get all registered agents
    /// </summary>
    [HttpGet]
  public IActionResult GetRegisteredAgents()
    {
        var agents = _agentOrchestrator.GetRegisteredAgents().Select(a => new
        {
            a.Name,
      Capabilities = a.Capabilities.Select(c => new
            {
    c.Name,
            c.Description,
 Type = c.Type.ToString()
})
        });

     return Ok(new { error = false, data = agents });
    }

/// <summary>
    /// Get available workflows
    /// </summary>
    [HttpGet("workflows")]
  public IActionResult GetAvailableWorkflows()
    {
  var workflows = _agentOrchestrator.GetAvailableWorkflows();
        return Ok(new { error = false, data = workflows });
    }

    /// <summary>
 /// Execute a single agent
    /// </summary>
    [HttpPost("{agentName}/execute")]
  public async Task<IActionResult> ExecuteAgent(string agentName, [FromBody] AgentExecutionRequest request)
    {
        var context = new AgentContext
        {
   UserId = _requestContext.GetCurrentUserId() ?? "anonymous",
         Parameters = request.Parameters,
      CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString()
     };

        var result = await _agentOrchestrator.ExecuteAgentAsync(agentName, context);

    if (result.Success)
    {
 return Ok(new { error = false, data = result });
        }

        return BadRequest(new { error = true, message = result.Message, errors = result.Errors });
    }

    /// <summary>
    /// Execute a predefined workflow
    /// </summary>
    [HttpPost("workflows/{workflowName}/execute")]
    public async Task<IActionResult> ExecuteWorkflow(string workflowName, [FromBody] WorkflowExecutionRequest request)
    {
        var context = new AgentContext
        {
 UserId = _requestContext.GetCurrentUserId() ?? "anonymous",
            Parameters = request.Parameters,
     CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString()
 };

        var result = await _agentOrchestrator.ExecuteWorkflowAsync(workflowName, context);

  if (result.Success)
        {
            return Ok(new { error = false, data = result });
        }

        var errors = result.AgentResults
            .Where(r => !r.Success)
        .SelectMany(r => r.Errors)
   .ToList();

return BadRequest(new { error = true, message = "Workflow execution failed", errors });
    }

    /// <summary>
    /// Process CV and create candidate profile (shortcut endpoint)
    /// </summary>
    [HttpPost("process-cv")]
    public async Task<IActionResult> ProcessCvAndCreateCandidate([FromBody] CvProcessingRequest request)
    {
        var context = new AgentContext
        {
   UserId = _requestContext.GetCurrentUserId() ?? "anonymous",
    Parameters = new Dictionary<string, object>
   {
      ["blobName"] = request.BlobName
            },
       CorrelationId = Guid.NewGuid().ToString()
        };

        var result = await _agentOrchestrator.ExecuteWorkflowAsync("cv-processing", context);

    if (result.Success)
        {
       // Extract the created candidate from the workflow result
  var candidateCreationResult = result.AgentResults.FirstOrDefault(r => r.AgentName == "candidate-creation-agent");
   if (candidateCreationResult?.Data.TryGetValue("candidate", out var candidateObj) == true)
 {
       return Ok(new { error = false, data = new
      {
       Candidate = candidateObj,
                WorkflowResult = result,
           JobMatches = result.AgentResults
  .FirstOrDefault(r => r.AgentName == "job-matching-agent")?.Data.GetValueOrDefault("jobMatches")
        } });
  }
    }

      var errors = result.AgentResults
      .Where(r => !r.Success)
       .SelectMany(r => r.Errors)
   .ToList();

     return BadRequest(new { error = true, message = "CV processing workflow failed", errors });
    }

    private string GetCurrentUserId() => _requestContext.GetCurrentUserId() ?? "anonymous";
}

// Request DTOs
public record AgentExecutionRequest
{
    public Dictionary<string, object> Parameters { get; init; } = [];
    public string? CorrelationId { get; init; }
}

public record WorkflowExecutionRequest
{
 public Dictionary<string, object> Parameters { get; init; } = [];
    public string? CorrelationId { get; init; }
}

public record CvProcessingRequest
{
    public string BlobName { get; init; } = string.Empty;
}