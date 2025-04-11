namespace RagApi.Models
{
    /// <summary>
    /// Represents a chunk of text from a document
    /// </summary>
    public class DocumentChunk
    {
        /// <summary>
        /// Unique identifier for the chunk
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Text content of the chunk
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Source identifier (e.g., file name)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Page number in the original document
        /// </summary>
        public int PageNumber { get; set; }
    }
}