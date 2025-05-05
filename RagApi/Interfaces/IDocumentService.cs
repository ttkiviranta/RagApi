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
        /// <param name="file">The document file to upload</param>
        /// <param name="documentType">Type of document (Resume, CoverLetter, JobPosting, etc.)</param>
        /// <param name="entityId">Optional ID of the entity this document belongs to</param>
        /// <param name="userId">Optional ID of the user who uploaded the document</param>
        Task<string> UploadAndProcessDocumentAsync(IFormFile file, string documentType, string? entityId, string? userId);

        /// <summary>
        /// Gets document content as bytes
        /// </summary>
        Task<byte[]> GetDocumentContentAsync(string documentId);

        /// <summary>
        /// Gets a document by ID
        /// </summary>
        /// <param name="documentId">ID of the document to retrieve</param>
        /// <returns>The document with the specified ID</returns>
        Task<Document> GetByIdAsync(string documentId);

        /// <summary>
        /// Gets documents by entity ID
        /// </summary>
        /// <param name="entityId">Optional entity ID to filter documents (null/empty returns all documents)</param>
        Task<IEnumerable<Document>> GetByEntityIdAsync(string? entityId);

        /// <summary>
        /// Deletes a document
        /// </summary>
        Task DeleteAsync(string documentId);
    }
}
