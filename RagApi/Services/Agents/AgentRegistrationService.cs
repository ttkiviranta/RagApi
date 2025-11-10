using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RagApi.Interfaces.Agents;
using RagApi.Services.Agents;

namespace RagApi.Services.Agents;

/// <summary>
/// Background service that registers all agents with the orchestrator during application startup
/// </summary>
public class AgentRegistrationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AgentRegistrationService> _logger;

    public AgentRegistrationService(IServiceProvider serviceProvider, ILogger<AgentRegistrationService> logger)
    {
_serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting agent registration process");

            // Get the orchestrator instance
            var orchestrator = _serviceProvider.GetRequiredService<IAgentOrchestrator>();

          // Register all agents using scoped services
     using var scope = _serviceProvider.CreateScope();
            
         var documentAgent = scope.ServiceProvider.GetRequiredService<DocumentAnalysisAgent>();
    orchestrator.RegisterAgent(documentAgent);

          var candidateAgent = scope.ServiceProvider.GetRequiredService<CandidateCreationAgent>();
     orchestrator.RegisterAgent(candidateAgent);

   var jobMatchingAgent = scope.ServiceProvider.GetRequiredService<JobMatchingAgent>();
       orchestrator.RegisterAgent(jobMatchingAgent);

   _logger.LogInformation("Successfully registered {AgentCount} agents with the orchestrator", 3);

      // Log available workflows
var workflows = orchestrator.GetAvailableWorkflows();
       _logger.LogInformation("Available workflows: {WorkflowNames}", 
   string.Join(", ", workflows.Select(w => w.Name)));
        }
        catch (Exception ex)
        {
_logger.LogError(ex, "Failed to register agents with orchestrator");
        throw;
        }

  // Complete immediately - this is a one-time registration task
        await Task.CompletedTask;
    }
}