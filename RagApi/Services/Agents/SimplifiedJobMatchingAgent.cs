using Microsoft.Extensions.Logging;
using RagApi.Interfaces;
using RagApi.Interfaces.Agents;
using RagApi.Models;
using RagApi.Models.Agents;
using RagApi.Models.Dto;
using System.Diagnostics;

namespace RagApi.Services.Agents;

/// <summary>
/// Agent for matching candidates with suitable job opportunities
/// </summary>
public class JobMatchingAgent : IAgent
{
    private readonly IJobMatchingService _jobMatchingService;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<JobMatchingAgent> _logger;

    public string Name => "job-matching-agent";

    public AgentCapability[] Capabilities => [
        new("Job Matching", "Match candidates with suitable job opportunities using AI", AgentCapabilityType.Matching),
        new("Skills Analysis", "Analyze skills compatibility", AgentCapabilityType.DataAnalysis)
    ];

    public JobMatchingAgent(
   IJobMatchingService jobMatchingService,
  IOpenAIService openAIService,
    ILogger<JobMatchingAgent> logger)
    {
        _jobMatchingService = jobMatchingService;
    _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new Dictionary<string, object>();
        var errors = new List<string>();

        try
        {
   _logger.LogInformation("Job matching agent starting for correlation {CorrelationId}", context.CorrelationId);

            // For new candidate matching (CV processing workflow)
            if (context.Parameters.ContainsKey("candidate"))
    {
   return await ExecuteNewCandidateMatching(context, stopwatch, result, errors);
            }
   // For existing candidate job matching
       else if (context.Parameters.ContainsKey("candidateText"))
            {
return await ExecuteCandidateTextMatching(context, stopwatch, result, errors);
            }
            else
            {
        errors.Add("Missing required parameters: candidate or candidateText");
       return CreateErrorResult("Invalid matching parameters", errors, stopwatch.Elapsed);
    }
        }
  catch (Exception ex)
        {
  _logger.LogError(ex, "Error in job matching agent for correlation {CorrelationId}", context.CorrelationId);
         errors.Add($"Job matching failed: {ex.Message}");
         return CreateErrorResult("Job matching failed", errors, stopwatch.Elapsed);
        }
    }

    private async Task<AgentResult> ExecuteNewCandidateMatching(AgentContext context, Stopwatch stopwatch,
        Dictionary<string, object> result, List<string> errors)
    {
    if (!context.Parameters.TryGetValue("candidate", out var candidateObj) ||
   candidateObj is not CandidateResponseDto candidate)
        {
   errors.Add("Invalid candidate object parameter");
            return CreateErrorResult("Invalid candidate data", errors, stopwatch.Elapsed);
        }

        var limit = context.Parameters.TryGetValue("limit", out var limitObj) && 
   int.TryParse(limitObj?.ToString(), out var l) ? l : 10;

  _logger.LogInformation("Matching newly created candidate {CandidateName} with available jobs", 
     $"{candidate.FirstName} {candidate.LastName}");

      // Create a candidate summary for matching
    var candidateText = CreateCandidateMatchingText(candidate);

        // Find matching jobs using the existing service (adapted to available methods)
   // For now, we'll create a simple matching based on the existing service structure
    var jobMatches = new List<JobMatchResult>(); // Placeholder - need to adapt based on available services
    
        result["candidateData"] = candidate;
  result["jobMatches"] = jobMatches;
        result["totalMatches"] = jobMatches.Count;
      result["candidateMatchingText"] = candidateText;
        result["aiInsights"] = await GenerateMatchingInsights(candidate, jobMatches);

   _logger.LogInformation("Found {MatchCount} job matches for new candidate {CandidateName}", 
     jobMatches.Count, $"{candidate.FirstName} {candidate.LastName}");

        return new AgentResult
        {
  Success = true,
          Message = $"Found {jobMatches.Count} job matches for new candidate {candidate.FirstName} {candidate.LastName}",
     Data = result,
     ExecutionTime = stopwatch.Elapsed,
            AgentName = Name
        };
    }

  private async Task<AgentResult> ExecuteCandidateTextMatching(AgentContext context, Stopwatch stopwatch,
        Dictionary<string, object> result, List<string> errors)
    {
        if (!context.Parameters.TryGetValue("candidateText", out var candidateTextObj) ||
      candidateTextObj is not string candidateText)
        {
 errors.Add("Invalid candidateText parameter");
     return CreateErrorResult("Invalid candidate text", errors, stopwatch.Elapsed);
        }

        var limit = context.Parameters.TryGetValue("limit", out var limitObj) && 
            int.TryParse(limitObj?.ToString(), out var l) ? l : 10;

        _logger.LogInformation("Matching candidate text with available jobs");

        // Find matching jobs using the existing service (placeholder for now)
  var jobMatches = new List<JobMatchResult>(); // Placeholder

        result["jobMatches"] = jobMatches;
        result["totalMatches"] = jobMatches.Count;
  result["candidateText"] = candidateText;

        _logger.LogInformation("Found {MatchCount} job matches for candidate text", jobMatches.Count);

        return new AgentResult
    {
         Success = true,
            Message = $"Found {jobMatches.Count} job matches for candidate",
            Data = result,
 ExecutionTime = stopwatch.Elapsed,
     AgentName = Name
   };
    }

  private string CreateCandidateMatchingText(CandidateResponseDto candidate)
    {
    var text = $"Candidate: {candidate.FirstName} {candidate.LastName}\n";
   
        if (!string.IsNullOrEmpty(candidate.CurrentPosition))
       text += $"Current Position: {candidate.CurrentPosition}\n";

        if (!string.IsNullOrEmpty(candidate.Skills))
       text += $"Skills: {candidate.Skills}\n";

        if (!string.IsNullOrEmpty(candidate.Location))
  text += $"Location: {candidate.Location}\n";

        return text;
    }

    private async Task<string> GenerateMatchingInsights(CandidateResponseDto candidate, List<JobMatchResult> jobMatches)
    {
        if (!jobMatches.Any())
        {
            return "No job matches found for this candidate profile.";
     }

        var topMatch = jobMatches.First();
        var prompt = $@"
Analyze job matching results for a candidate:

Candidate Profile:
- Name: {candidate.FirstName} {candidate.LastName}
- Current Position: {candidate.CurrentPosition ?? "Not specified"}
- Skills: {candidate.Skills ?? "Not specified"}

Top Job Match:
- Score: {topMatch.Score:F2}
- Total Matches Found: {jobMatches.Count}

Provide a brief 2-3 sentence insight about the candidate's job market prospects and match quality.";

        try
        {
         return await _openAIService.GetChatCompletionsAsync(
    "You are an expert HR analyst providing job matching insights.", prompt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate AI insights");
            return $"Found {jobMatches.Count} potential job matches with top match score of {topMatch.Score:F2}.";
        }
    }

    private AgentResult CreateErrorResult(string message, List<string> errors, TimeSpan executionTime)
    {
        return new AgentResult
        {
            Success = false,
    Message = message,
         Errors = errors,
   ExecutionTime = executionTime,
          AgentName = Name
        };
    }
}