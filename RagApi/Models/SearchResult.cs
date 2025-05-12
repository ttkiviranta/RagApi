namespace RagApi.Models
{
    /// <summary>
    /// Represents a search result from vector search
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// Unique identifier of the document chunk
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Text content of the document chunk
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Source identifier (e.g., file name)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Relevance score from the search
        /// </summary>
        public double Score { get; set; }
    }
}
