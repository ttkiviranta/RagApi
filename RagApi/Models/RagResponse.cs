using System.Collections.Generic;

namespace RagApi.Models
{
    /// <summary>
    /// Response from a RAG query
    /// </summary>
    public class RagResponse
    {
        /// <summary>
        /// Generated answer to the query
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Source search results used to generate the answer
        /// </summary>
        public List<SearchResult> SourceResults { get; set; }
    }
}
