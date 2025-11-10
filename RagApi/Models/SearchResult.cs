namespace RagApi.Models;

/// <summary>
/// Represents a search result from vector search
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Unique identifier of the document chunk
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Text content of the document chunk
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Source identifier (e.g., file name)
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Relevance score from the search
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Gets the score as a percentage value
    /// </summary>
    public int ScorePercentage => (int)(Score * 100);

    /// <summary>
    /// Returns a shortened preview of the content
    /// </summary>
    public string GetContentPreview(int maxLength = 100) =>
        string.IsNullOrEmpty(Content) ? string.Empty :
            (Content.Length <= maxLength ? Content : $"{Content[..maxLength]}...");
}
