using System.Collections.Generic;

namespace RagApi.Models;

/// <summary>
/// Response from a RAG query
/// </summary>
public class RagResponse
{
    /// <summary>
    /// Generated answer to the query
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Source search results used to generate the answer
    /// </summary>
    public List<SearchResult> SourceResults { get; set; } = new List<SearchResult>();

    /// <summary>
    /// Checks if the response has any source results
    /// </summary>
    public bool HasSourceResults => SourceResults?.Count > 0;
}
