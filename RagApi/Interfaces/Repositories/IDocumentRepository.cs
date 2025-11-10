// RagApi/Interfaces/Repositories/IDocumentRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces.Repositories
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetByEntityIdAsync(string entityId);
        Task<IEnumerable<Document>> GetByDocumentTypeAsync(string documentType);
        Task DeleteWithBlobAsync(string documentId);
        Task UpdateMetadataAsync(string documentId, string metadata);
    }
}
