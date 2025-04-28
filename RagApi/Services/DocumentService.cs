using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    /// <summary>
    /// Implementation of document service interface
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDocumentIntelligenceService _documentIntelligenceService;
        private readonly IVectorSearchService _searchService;

        public DocumentService(
            ApplicationDbContext context,
            IBlobStorageService blobStorageService,
            IDocumentIntelligenceService documentIntelligenceService,
            IVectorSearchService searchService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _documentIntelligenceService = documentIntelligenceService;
            _searchService = searchService;
        }

        /// <inheritdoc/>
        public async Task<string> UploadAndProcessDocumentAsync(
            IFormFile file,
            string documentType,
            string entityId,
            string userId)
        {
            // Validate file type
            if (file.ContentType != "application/pdf")
                throw new ArgumentException("Only PDF documents are supported.");

            // Create unique name for the file
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            // Save file to Azure Blob Storage
            var blobPath = await _blobStorageService.UploadFileAsync(file.OpenReadStream(), fileName, file.ContentType);

            // Create document entity
            var document = new Document
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                BlobStoragePath = blobPath,
                DocumentType = documentType,
                ContentType = file.ContentType,
                EntityId = entityId,
                Metadata = "{}",
                UploadedDate = DateTime.UtcNow,
                UploadedByUserId = userId
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Process document asynchronously
            _ = Task.Run(async () => {
                try
                {
                    // Get file from Blob Storage
                    var fileContent = await _blobStorageService.DownloadFileAsync(blobPath);

                    // Analyze document text using Azure Document Intelligence
                    var extractedText = await _documentIntelligenceService.ExtractTextFromPdfAsync(fileContent);

                    // Index document for search
                    await _searchService.IndexDocumentAsync(document.Id, documentType, extractedText.Content, entityId);

                    // Update metadata information
                    document.Metadata = JsonSerializer.Serialize(new
                    {
                        pageCount = extractedText.Pages.Count,
                        charCount = extractedText.Content.Length,
                        indexed = true
                    });

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Error handling - could log to a database or create an error entity
                    document.Metadata = JsonSerializer.Serialize(new
                    {
                        error = ex.Message,
                        indexed = false
                    });

                    await _context.SaveChangesAsync();
                }
            });

            return document.Id;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetDocumentContentAsync(string documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException("Document not found");

            return await _blobStorageService.DownloadFileAsync(document.BlobStoragePath);
        }

        /// <inheritdoc/>
        public async Task<Document> GetByIdAsync(string documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException("Document not found");

            return document;
        }

        /// <inheritdoc/>
        public async Task<List<Document>> GetByEntityIdAsync(string entityId)
        {
            return await _context.Documents
                .Where(d => d.EntityId == entityId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException("Document not found");

            // Delete from blob storage
            await _blobStorageService.DeleteFileAsync(document.BlobStoragePath);

            // Delete from search index
            await _searchService.DeleteDocumentAsync(document.Id);

            // Delete document from database
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}
