using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DocumentService(
            ApplicationDbContext context,
            IBlobStorageService blobStorageService,
            IDocumentIntelligenceService documentIntelligenceService,
            IVectorSearchService searchService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _documentIntelligenceService = documentIntelligenceService;
            _searchService = searchService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc/>
        public async Task<string> UploadAndProcessDocumentAsync(
            IFormFile file,
            string documentType,
            string? entityId,
            string? userId)
        {
            // Validate file type
            if (file.ContentType != "application/pdf")
                throw new ArgumentException("Only PDF documents are supported.");

            // Validate document type
            var validTypes = new[] { "Resume", "CoverLetter", "JobPosting", "Other" };
            if (!validTypes.Contains(documentType, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid document type. Allowed types are: {string.Join(", ", validTypes)}");

            // Normalize document type to match backend conventions
            documentType = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(documentType.ToLower());

            // If userId is provided, verify user exists
            if (!string.IsNullOrEmpty(userId))
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    // User doesn't exist, set userId to null
                    userId = null;
                }
            }

            // Create unique name for the file
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            // Save file to Azure Blob Storage
            var blobPath = await _blobStorageService.UploadFileAsync(file.OpenReadStream(), fileName, file.ContentType);

            // Create document entity with nullable fields
            var document = new Document
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                BlobStoragePath = blobPath,
                DocumentType = documentType,
                ContentType = file.ContentType,
                EntityId = entityId ?? string.Empty,
                Metadata = "{}",
                UploadedDate = DateTime.UtcNow,
                UploadedByUserId = userId ?? string.Empty
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            string documentId = document.Id;
            string savedDocumentType = document.DocumentType;
            string savedBlobPath = document.BlobStoragePath;
            string? savedEntityId = entityId;

            // Process document asynchronously
            _ = Task.Run(async () => {
                // Käytä uutta scopia tausta-ajossa
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        // Hae uusi DbContext-instanssi scopesta
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Etsi dokumentti uudesta kontekstista
                        var doc = await dbContext.Documents.FindAsync(documentId);
                        if (doc == null)
                        {
                            // Dokumenttia ei löytynyt, ei voida jatkaa
                            return;
                        }

                        // Get file from Blob Storage
                        var fileContent = await _blobStorageService.DownloadFileAsync(savedBlobPath);

                        // Analyze document text using Azure Document Intelligence
                        var extractedText = await _documentIntelligenceService.ExtractTextFromPdfAsync(fileContent);

                        // Index document for search
                        await _searchService.IndexDocumentAsync(documentId, savedDocumentType, extractedText.Content, savedEntityId ?? string.Empty);

                        // Update metadata information
                        doc.Metadata = JsonSerializer.Serialize(new
                        {
                            pageCount = extractedText.Pages.Count,
                            charCount = extractedText.Content.Length,
                            indexed = true
                        });

                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Kokeile hakea dokumentti uudestaan virheen tapahtuessa
                        try
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var doc = await dbContext.Documents.FindAsync(documentId);
                            if (doc != null)
                            {
                                // Error handling - could log to a database or create an error entity
                                doc.Metadata = JsonSerializer.Serialize(new
                                {
                                    error = ex.Message,
                                    indexed = false
                                });

                                await dbContext.SaveChangesAsync();
                            }
                        }
                        catch
                        {
                            // Jätä virhe käsittelemättä, jos dokumentin päivitys ei onnistu
                        }
                    }
                }
            });

            return documentId;
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
        public async Task<IEnumerable<Document>> GetByEntityIdAsync(string? entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                // Fetch all documents if entityId is null or empty
                var allDocuments = await _context.Documents.ToListAsync();
                if (!allDocuments.Any())
                {
                    throw new KeyNotFoundException("No documents found.");
                }
                return allDocuments;
            }

            // Fetch documents for the specific entityId
            var documents = await _context.Documents.Where(d => d.EntityId == entityId).ToListAsync();
            if (!documents.Any())
            {
                throw new KeyNotFoundException($"No documents found for entity ID: {entityId}");
            }

            return documents;
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
