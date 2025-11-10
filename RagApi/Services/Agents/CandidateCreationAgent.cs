using Microsoft.Extensions.Logging;
using RagApi.Interfaces;
using RagApi.Interfaces.Agents;
using RagApi.Models;
using RagApi.Models.Agents;
using RagApi.Models.Dto;
using System.Diagnostics;

namespace RagApi.Services.Agents;

/// <summary>
/// Agent for creating candidate profiles from extracted document data
/// </summary>
public class CandidateCreationAgent : IAgent
{
    private readonly ICandidateService _candidateService;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<CandidateCreationAgent> _logger;

    public string Name => "candidate-creation-agent";

    public AgentCapability[] Capabilities => [
        new("Candidate Profile Creation", "Create structured candidate profiles from CV data", AgentCapabilityType.DataAnalysis),
        new("Information Extraction", "Extract candidate information using AI", AgentCapabilityType.DataAnalysis),
        new("Data Validation", "Validate and clean candidate data", AgentCapabilityType.DataAnalysis),
        new("Profile Enrichment", "Enrich candidate profiles with additional insights", AgentCapabilityType.DataAnalysis)
    ];

    public CandidateCreationAgent(
        ICandidateService candidateService,
  IOpenAIService openAIService,
      ILogger<CandidateCreationAgent> logger)
    {
  _candidateService = candidateService;
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
       _logger.LogInformation("Candidate creation agent starting for correlation {CorrelationId}", context.CorrelationId);

       // Get extracted document data from previous agent
  if (!context.Parameters.TryGetValue("extractedChunks", out var chunksObj) || 
      chunksObj is not List<DocumentChunk> documentChunks)
          {
     errors.Add("Missing extracted document chunks from previous agent");
      return CreateErrorResult("Missing document data", errors, stopwatch.Elapsed);
            }

            // Get content analysis from previous agent
            if (!context.Parameters.TryGetValue("contentAnalysis", out var analysisObj) || 
      analysisObj is not Dictionary<string, object> contentAnalysis)
            {
      _logger.LogWarning("Content analysis not available from previous agent");
     contentAnalysis = new Dictionary<string, object>();
    }

  // Combine all text content
            var fullText = string.Join(" ", documentChunks.Select(c => c.Content));

        _logger.LogInformation("Processing candidate data from {ChunkCount} document chunks", documentChunks.Count);

    // Step 1: Extract structured candidate information using AI
       var candidateInfo = await ExtractCandidateInformation(fullText);
  result["extractedCandidateInfo"] = candidateInfo;

     // Step 2: Validate and clean the extracted data
            var validatedInfo = ValidateCandidateData(candidateInfo, contentAnalysis);
      result["validatedCandidateInfo"] = validatedInfo;

        // Step 3: Create candidate DTO for creation
 var candidateDto = CreateCandidateCreateDto(validatedInfo);
   
        // Step 4: Save candidate to database
        var createdCandidate = await _candidateService.CreateAsync(candidateDto, context.UserId);
            result["candidateId"] = createdCandidate.Id;
      result["candidate"] = createdCandidate;

  _logger.LogInformation("Created candidate {CandidateId} with name {CandidateName}", 
        createdCandidate.Id, createdCandidate.FullName);

            return new AgentResult
            {
       Success = true,
                Message = $"Successfully created candidate profile for {createdCandidate.FullName}",
       Data = result,
       ExecutionTime = stopwatch.Elapsed,
      AgentName = Name
            };
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Error in candidate creation agent for correlation {CorrelationId}", context.CorrelationId);
  errors.Add($"Candidate creation failed: {ex.Message}");
   return CreateErrorResult("Candidate creation failed", errors, stopwatch.Elapsed);
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

    private async Task<Dictionary<string, object>> ExtractCandidateInformation(string cvText)
    {
        var prompt = $@"
Extract the following information from this CV/Resume text. Return the information in a structured format.

CV Text:
{cvText}

Please extract:
1. Full Name
2. Email Address  
3. Phone Number
4. Skills (list of skills mentioned)
5. Years of Experience (estimate based on work history)
6. Education (highest degree and field)
7. Current/Most Recent Job Title
8. Summary (brief 2-3 sentence summary of the candidate)

Format your response as key-value pairs, one per line:
Name: [extracted name]
Email: [extracted email]
Phone: [extracted phone]
Skills: [comma-separated list of skills]
Experience: [number of years]
Education: [degree and field]
JobTitle: [current/recent title]
Summary: [brief summary]

If any information is not clearly available, use 'Not specified' for that field.
";

      try
        {
            var aiResponse = await _openAIService.GetChatCompletionsAsync(
                "You are an expert at extracting structured information from CVs and resumes. Be precise and only extract information that is clearly stated.",
                prompt);

            return ParseAIResponse(aiResponse);
        }
    catch (Exception ex)
   {
            _logger.LogWarning(ex, "AI extraction failed, falling back to basic parsing");
            return ParseCVBasic(cvText);
        }
 }

    private Dictionary<string, object> ParseAIResponse(string aiResponse)
    {
        var result = new Dictionary<string, object>();
        
        var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
  
        foreach (var line in lines)
        {
            var colonIndex = line.IndexOf(':');
      if (colonIndex > 0 && colonIndex < line.Length - 1)
         {
      var key = line.Substring(0, colonIndex).Trim();
              var value = line.Substring(colonIndex + 1).Trim();
          
   // Clean up the value
          if (value.StartsWith("[") && value.EndsWith("]"))
    {
           value = value.Substring(1, value.Length - 2);
  }
         
 if (!string.IsNullOrEmpty(value) && value != "Not specified")
        {
    result[key] = value;
        }
        }
     }

        return result;
    }

    private Dictionary<string, object> ParseCVBasic(string cvText)
    {
     var result = new Dictionary<string, object>();

     // Basic email extraction
      var emailMatch = System.Text.RegularExpressions.Regex.Match(cvText, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");
     if (emailMatch.Success)
  {
            result["Email"] = emailMatch.Value;
        }

        // Basic phone extraction
        var phoneMatch = System.Text.RegularExpressions.Regex.Match(cvText, @"[\+]?[1-9]?[\d\s\-\(\)]{8,15}");
     if (phoneMatch.Success)
        {
            result["Phone"] = phoneMatch.Value.Trim();
      }

        // Estimate experience years (very basic)
var currentYear = DateTime.Now.Year;
        var yearMatches = System.Text.RegularExpressions.Regex.Matches(cvText, @"\b(19|20)\d{2}\b");
        if (yearMatches.Count > 0)
        {
    var earliestYear = yearMatches.Select(m => int.Parse(m.Value)).Min();
  if (earliestYear > 1990 && earliestYear < currentYear)
            {
          result["Experience"] = (currentYear - earliestYear).ToString();
            }
        }

        return result;
    }

    private Dictionary<string, object> ValidateCandidateData(Dictionary<string, object> candidateInfo, Dictionary<string, object> contentAnalysis)
    {
        var validated = new Dictionary<string, object>(candidateInfo);

        // Validate email
 if (validated.TryGetValue("Email", out var emailObj) && emailObj is string email)
        {
            if (!IsValidEmail(email))
            {
              validated.Remove("Email");
        _logger.LogWarning("Invalid email format detected and removed: {Email}", email);
            }
}

        // Validate experience years
        if (validated.TryGetValue("Experience", out var expObj) && expObj is string expStr)
        {
   if (int.TryParse(expStr, out var years) && (years < 0 || years > 60))
            {
    validated.Remove("Experience");
 _logger.LogWarning("Unrealistic experience years detected and removed: {Years}", years);
            }
        }

        // Use content analysis to enrich data
        if (contentAnalysis.TryGetValue("emailAddresses", out var emailsObj) && emailsObj is List<string> emails)
        {
  if (emails.Any() && !validated.ContainsKey("Email"))
   {
      validated["Email"] = emails.First();
    }
    }

     return validated;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
 return addr.Address == email;
        }
        catch
        {
            return false;
  }
    }

    private CandidateCreateDto CreateCandidateCreateDto(Dictionary<string, object> candidateInfo)
    {
        var dto = new CandidateCreateDto();

  // Extract name
        if (candidateInfo.TryGetValue("Name", out var nameObj))
  {
          var fullName = nameObj.ToString() ?? "";
            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length > 0)
     {
        dto.FirstName = nameParts[0];
       if (nameParts.Length > 1)
      {
  dto.LastName = string.Join(" ", nameParts.Skip(1));
    }
   }
        }

   // Set other properties that exist in CandidateCreateDto
     if (candidateInfo.TryGetValue("Email", out var emailObj))
  dto.Email = emailObj.ToString() ?? "";

        if (candidateInfo.TryGetValue("Phone", out var phoneObj))
      dto.PhoneNumber = phoneObj.ToString() ?? "";

        if (candidateInfo.TryGetValue("Skills", out var skillsObj))
   dto.Skills = skillsObj.ToString() ?? "";

        if (candidateInfo.TryGetValue("JobTitle", out var titleObj))
            dto.CurrentPosition = titleObj.ToString() ?? "";

        // Set default values if not provided
        dto.FirstName ??= "Unknown";
  dto.LastName ??= "Candidate";
        dto.Email ??= "";
        dto.PhoneNumber ??= "";
        dto.Skills ??= "";
        dto.CurrentPosition ??= "";

  return dto;
    }
}