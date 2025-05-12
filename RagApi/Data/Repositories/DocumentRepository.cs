// RagApi/Data/Repositories/DocumentRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Interfaces;
using RagApi.Interfaces.Repositories;
using RagApi.Models;

namespace RagApi.Data.Repositories
{
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IVectorSearchService _searchService;

        public DocumentRepository(
            ApplicationDbContext context,
            IBlobStorageService blobStorageService,
            IVectorSearchService searchService) : base(context)
        {
            _blobStorageService = blobStorageService;
            _searchService = searchService;
        }

        public async Task<IEnumerable<Document>> GetByEntityIdAsync(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                return await GetAllAsync();
            }

            return await _dbSet.Where(d => d.EntityId == entityId).ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByDocumentTypeAsync(string documentType)
        {
            return await _dbSet.Where(d => d.DocumentType == documentType).ToListAsync();
        }

        public async Task DeleteWithBlobAsync(string documentId)
        {
            var document = await GetByIdAsync(documentId);
            if (document != null)
            {
                // Delete blob
                await _blobStorageService.DeleteFileAsync(document.BlobStoragePath);

                // Delete from search index
                await _searchService.DeleteDocumentAsync(document.Id);

                // Delete document entity
                await DeleteAsync(document);
                await SaveChangesAsync();
            }
        }

        public async Task UpdateMetadataAsync(string documentId, string metadata)
        {
            var document = await GetByIdAsync(documentId);
            if (document != null)
            {
                document.Metadata = metadata;
                await SaveChangesAsync();
            }
        }
    }
}
