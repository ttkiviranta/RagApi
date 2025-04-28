using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service for vector-based document search
    /// </summary>
    public interface IVectorSearchService
    {
        /// <summary>
        /// Indexes document chunks for vector search
        /// </summary>
        /// <param name="chunks">List of document chunks to index</param>
        Task IndexDocumentChunksAsync(List<DocumentChunk> chunks);

        /// <summary>
        /// Performs semantic search based on query
        /// </summary>
        /// <param name="query">Search query text</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>List of search results ordered by relevance</returns>
        Task<List<SearchResult>> SearchAsync(string query, int maxResults = 5);
        /// <summary>
        /// Indexes a document for vector search
        /// </summary>
        Task IndexDocumentAsync(string documentId, string documentType, string content, string entityId);

        /// <summary>
        /// Searches for documents based on a query
        /// </summary>
        Task<List<SearchResult>> SearchDocumentsAsync(string query, int maxResults = 5);

        /// <summary>
        /// Deletes a document from the search index
        /// </summary>
        Task DeleteDocumentAsync(string documentId);
    }
}
