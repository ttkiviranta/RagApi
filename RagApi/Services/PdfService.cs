using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using RagApi.Interfaces;
using RagApi.Models;
using Azure;

namespace RagApi.Services
{
    public class PdfService : IPdfService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly DocumentAnalysisClient _documentAnalysisClient;
        private readonly string _containerName = "pdfs";

        public PdfService(BlobServiceClient blobServiceClient, DocumentAnalysisClient documentAnalysisClient)
        {
            _blobServiceClient = blobServiceClient;
            _documentAnalysisClient = documentAnalysisClient;

            // Ensure container exists
            _blobServiceClient.GetBlobContainerClient(_containerName).CreateIfNotExists();
        }

        public async Task<string> UploadPdfAsync(Stream pdfStream, string fileName)
        {
            // Create a unique name for the file
            string blobName = $"{Guid.NewGuid()}-{fileName}";

            // Get reference to blob container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload file to Blob Storage
            await blobClient.UploadAsync(pdfStream, true);

            return blobName;
        }

        public async Task<List<DocumentChunk>> ExtractTextFromPdfAsync(string blobName)
        {
            // Get PDF file from Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Read PDF file into memory
            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;

            // Use Document Analysis service to extract text
            var operation = await _documentAnalysisClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", memoryStream);
            var result = operation.Value;

            // Split text into chunks
            var chunks = new List<DocumentChunk>();

            foreach (var page in result.Pages)
            {
                // Get all content from this page
                StringBuilder pageContent = new StringBuilder();

                // Since content is not directly accessible per page in newer API versions,
                // we need to collect paragraphs that belong to this page
                foreach (var paragraph in result.Paragraphs)
                {
                    if (paragraph.BoundingRegions.Count > 0 &&
                        paragraph.BoundingRegions[0].PageNumber == page.PageNumber)
                    {
                        pageContent.AppendLine(paragraph.Content);
                    }
                }

                // Create a chunk for this page
                var chunk = new DocumentChunk
                {
                    Id = $"{blobName}-page-{page.PageNumber}",
                    Content = pageContent.ToString().Trim(),
                    Source = blobName,
                    PageNumber = page.PageNumber
                };

                chunks.Add(chunk);
            }

            return chunks;
        }
    }
}