using System.Collections.Generic;

namespace RagApi.Models
{
    /// <summary>
    /// Represents the result of document analysis
    /// </summary>
    public class DocumentAnalysisResult
    {
        /// <summary>
        /// Full content of the document
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// List of pages in the document
        /// </summary>
        public List<DocumentPage> Pages { get; set; } = new List<DocumentPage>();
    }

    /// <summary>
    /// Represents a page in the document
    /// </summary>
    public class DocumentPage
    {
        /// <summary>
        /// Page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Text content of the page
        /// </summary>
        public string Content { get; set; }
    }
}
