using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    /// <summary>
    /// Implementation of document service interface
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDocumentIntelligenceService _documentIntelligenceService;
        private readonly IVectorSearchService _searchService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DocumentService(
            IUnitOfWork unitOfWork,
            IBlobStorageService blobStorageService,
            IDocumentIntelligenceService documentIntelligenceService,
            IVectorSearchService searchService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
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
                EntityId = entityId ?? string.Empty,
                Metadata = "{}",
                UploadedDate = DateTime.UtcNow,
                UploadedByUserId = userId
            };

            await _unitOfWork.Documents.AddAsync(document);
            await _unitOfWork.CommitAsync();

            string documentId = document.Id;
            string savedDocumentType = document.DocumentType;
            string savedBlobPath = document.BlobStoragePath;
            string? savedEntityId = entityId;

            // Process document asynchronously
            _ = Task.Run(async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        // Hae uusi UnitOfWork-instanssi scopesta
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        // Etsi dokumentti uudesta kontekstista
                        var doc = await unitOfWork.Documents.GetByIdAsync(documentId);
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
                        await unitOfWork.Documents.UpdateMetadataAsync(documentId, JsonSerializer.Serialize(new
                        {
                            pageCount = extractedText.Pages.Count,
                            charCount = extractedText.Content.Length,
                            indexed = true
                        }));

                        await unitOfWork.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                            var doc = await unitOfWork.Documents.GetByIdAsync(documentId);
                            if (doc != null)
                            {
                                // Update error metadata
                                await unitOfWork.Documents.UpdateMetadataAsync(documentId, JsonSerializer.Serialize(new
                                {
                                    error = ex.Message,
                                    indexed = false
                                }));

                                await unitOfWork.CommitAsync();
                            }
                        }
                        catch
                        {
                            // Handle errors in error handling... just log or ignore
                        }
                    }
                }
            });

            return documentId;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetDocumentContentAsync(string documentId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException("Document not found");

            return await _blobStorageService.DownloadFileAsync(document.BlobStoragePath);
        }

        /// <inheritdoc/>
        public async Task<Document> GetByIdAsync(string documentId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException("Document not found");

            return document;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Document>> GetByEntityIdAsync(string? entityId)
        {
            return await _unitOfWork.Documents.GetByEntityIdAsync(entityId);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string documentId)
        {
            await _unitOfWork.Documents.DeleteWithBlobAsync(documentId);
        }
    }
}
