using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RagApi.Models;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service interface for managing document operations
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Uploads and processes a document
        /// </summary>
        Task<string> UploadAndProcessDocumentAsync(IFormFile file, string documentType, string entityId, string userId);

        /// <summary>
        /// Gets document content as bytes
        /// </summary>
        Task<byte[]> GetDocumentContentAsync(string documentId);

        /// <summary>
        /// Gets a document by ID
        /// </summary>
        Task<Document> GetByIdAsync(string documentId);

        /// <summary>
        /// Gets documents by entity ID
        /// </summary>
        Task<List<Document>> GetByEntityIdAsync(string entityId);

        /// <summary>
        /// Deletes a document
        /// </summary>
        Task DeleteAsync(string documentId);
    }
}
