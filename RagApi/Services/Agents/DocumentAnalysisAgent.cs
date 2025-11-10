using Microsoft.Extensions.Logging;
using RagApi.Interfaces;
using RagApi.Interfaces.Agents;
using RagApi.Models.Agents;
using System.Diagnostics;

namespace RagApi.Services.Agents;

/// <summary>
/// Agent for analyzing documents and extracting structured information
/// </summary>
public class DocumentAnalysisAgent : IAgent
{
    private readonly IDocumentIntelligenceService _documentService;
    private readonly IPdfService _pdfService;
private readonly ILogger<DocumentAnalysisAgent> _logger;

    public string Name => "document-analysis-agent";

    public AgentCapability[] Capabilities => [
new("PDF Text Extraction", "Extract text content from PDF documents", AgentCapabilityType.DocumentProcessing),
        new("Document Classification", "Classify document types (CV, Job Posting, etc.)", AgentCapabilityType.DataAnalysis),
        new("Metadata Extraction", "Extract structured metadata from documents", AgentCapabilityType.DocumentProcessing),
  new("Content Analysis", "Analyze document content and structure", AgentCapabilityType.DataAnalysis)
    ];

    public DocumentAnalysisAgent(
        IDocumentIntelligenceService documentService,
      IPdfService pdfService,
   ILogger<DocumentAnalysisAgent> logger)
    {
 _documentService = documentService;
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
     var stopwatch = Stopwatch.StartNew();
      var result = new Dictionary<string, object>();
    var errors = new List<string>();

        try
        {
            _logger.LogInformation("Document analysis agent starting for correlation {CorrelationId}", context.CorrelationId);

  // Get required parameters
            if (!context.Parameters.TryGetValue("blobName", out var blobNameObj) || blobNameObj is not string blobName)
            {
    errors.Add("Missing required parameter: blobName");
     return CreateErrorResult("Missing blobName parameter", errors, stopwatch.Elapsed);
            }

            // Get document type if specified
            var documentType = context.Parameters.TryGetValue("documentType", out var docTypeObj) 
       ? docTypeObj?.ToString() ?? "Unknown" 
        : "Unknown";

            _logger.LogInformation("Analyzing document {BlobName} of type {DocumentType}", blobName, documentType);

            // Step 1: Extract text from document
          var documentChunks = await _pdfService.ExtractTextFromPdfAsync(blobName);
   if (!documentChunks.Any())
            {
                errors.Add("No text content could be extracted from the document");
   return CreateErrorResult("Document text extraction failed", errors, stopwatch.Elapsed);
            }

         result["extractedChunks"] = documentChunks;
          result["totalChunks"] = documentChunks.Count;
      result["totalTextLength"] = documentChunks.Sum(c => c.Content.Length);

     _logger.LogInformation("Extracted {ChunkCount} text chunks from document {BlobName}", 
     documentChunks.Count, blobName);

    // Step 2: Analyze document structure and content
    var fullText = string.Join(" ", documentChunks.Select(c => c.Content));
         var analysisResult = await AnalyzeDocumentContent(fullText, documentType);
         
     result["contentAnalysis"] = analysisResult;
            result["documentType"] = documentType;
  result["blobName"] = blobName;

    // Step 3: Extract metadata based on document type
            var metadata = await ExtractDocumentMetadata(fullText, documentType);
            result["extractedMetadata"] = metadata;

            _logger.LogInformation("Document analysis completed for {BlobName} in {ElapsedMs}ms", 
      blobName, stopwatch.ElapsedMilliseconds);

         return new AgentResult
            {
         Success = true,
    Message = $"Successfully analyzed document {blobName}",
    Data = result,
            ExecutionTime = stopwatch.Elapsed,
 AgentName = Name
     };
   }
    catch (Exception ex)
     {
      _logger.LogError(ex, "Error in document analysis agent for correlation {CorrelationId}", context.CorrelationId);
            errors.Add($"Document analysis failed: {ex.Message}");
       return CreateErrorResult("Document analysis failed", errors, stopwatch.Elapsed);
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

    private async Task<Dictionary<string, object>> AnalyzeDocumentContent(string text, string documentType)
    {
 var analysis = new Dictionary<string, object>();

        // Basic content analysis
        analysis["wordCount"] = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        analysis["characterCount"] = text.Length;
        analysis["estimatedReadingTime"] = TimeSpan.FromMinutes(text.Split(' ').Length / 200.0); // Average reading speed

 // Document type specific analysis
        switch (documentType.ToLower())
        {
         case "cv":
    case "resume":
   analysis.Merge(await AnalyzeCVContent(text));
  break;
         case "jobposting":
              analysis.Merge(await AnalyzeJobPostingContent(text));
       break;
 case "coverletter":
         analysis.Merge(await AnalyzeCoverLetterContent(text));
          break;
        }

        return analysis;
    }

    private async Task<Dictionary<string, object>> AnalyzeCVContent(string text)
    {
        var analysis = new Dictionary<string, object>();
        var lowerText = text.ToLower();

  // Look for common CV sections
        analysis["hasContactInfo"] = lowerText.Contains("email") || lowerText.Contains("phone") || lowerText.Contains("@");
        analysis["hasExperience"] = lowerText.Contains("experience") || lowerText.Contains("work") || lowerText.Contains("employment");
    analysis["hasEducation"] = lowerText.Contains("education") || lowerText.Contains("degree") || lowerText.Contains("university");
    analysis["hasSkills"] = lowerText.Contains("skills") || lowerText.Contains("competencies");

        // Extract potential email addresses
        var emailMatches = System.Text.RegularExpressions.Regex.Matches(text, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");
        analysis["emailAddresses"] = emailMatches.Select(m => m.Value).ToList();

// Extract potential phone numbers (basic pattern)
        var phoneMatches = System.Text.RegularExpressions.Regex.Matches(text, @"[\+]?[1-9]?[\d\s\-\(\)]{8,15}");
        analysis["phoneNumbers"] = phoneMatches.Select(m => m.Value.Trim()).Where(p => p.Length >= 8).ToList();

        return analysis;
    }

 private async Task<Dictionary<string, object>> AnalyzeJobPostingContent(string text)
    {
        var analysis = new Dictionary<string, object>();
        var lowerText = text.ToLower();

// Look for job posting sections
      analysis["hasRequirements"] = lowerText.Contains("requirements") || lowerText.Contains("qualifications");
        analysis["hasResponsibilities"] = lowerText.Contains("responsibilities") || lowerText.Contains("duties");
        analysis["hasSalary"] = lowerText.Contains("salary") || lowerText.Contains("compensation") || lowerText.Contains("€") || lowerText.Contains("$");
   analysis["hasLocation"] = lowerText.Contains("location") || lowerText.Contains("office") || lowerText.Contains("remote");

        // Extract potential salary information
        var salaryMatches = System.Text.RegularExpressions.Regex.Matches(text, @"[€$]\s*\d+[\d\s,]*|\d+[\d\s,]*\s*[€$]");
        analysis["salaryMentions"] = salaryMatches.Select(m => m.Value.Trim()).ToList();

        return analysis;
    }

    private async Task<Dictionary<string, object>> AnalyzeCoverLetterContent(string text)
    {
 var analysis = new Dictionary<string, object>();
        var lowerText = text.ToLower();

    // Cover letter specific analysis
 analysis["hasGreeting"] = lowerText.Contains("dear") || lowerText.Contains("hello");
        analysis["hasClosing"] = lowerText.Contains("sincerely") || lowerText.Contains("regards") || lowerText.Contains("yours");
      analysis["mentionsPosition"] = lowerText.Contains("position") || lowerText.Contains("job") || lowerText.Contains("role");

     return analysis;
  }

    private async Task<Dictionary<string, object>> ExtractDocumentMetadata(string text, string documentType)
    {
     var metadata = new Dictionary<string, object>
        {
  ["documentType"] = documentType,
   ["extractedAt"] = DateTime.UtcNow,
            ["contentLength"] = text.Length
        };

     // Add type-specific metadata
        switch (documentType.ToLower())
  {
 case "cv":
          case "resume":
      metadata["category"] = "Candidate Document";
        break;
       case "jobposting":
     metadata["category"] = "Job Document";
    break;
 case "coverletter":
          metadata["category"] = "Application Document";
     break;
      default:
                metadata["category"] = "General Document";
    break;
        }

        return metadata;
    }
}

// Extension method for merging dictionaries
public static class DictionaryExtensions
{
    public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> first, Dictionary<TKey, TValue> second)
    {
      foreach (var kvp in second)
        {
      first[kvp.Key] = kvp.Value;
     }
    }
}