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
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RagApi.Services
{
    public class PdfService : IPdfService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly DocumentAnalysisClient _documentAnalysisClient;
        private readonly IMessageBusService _messageBusService;
        private readonly ILogger<PdfService> _logger;
        private readonly string _containerName = "pdfs";
        private const string PDF_QUEUE_NAME = "pdf-processing-queue";

        /// <summary>
        /// Constructor for the PDF service
        /// </summary>
        /// <param name="blobServiceClient">Azure Blob Storage client</param>
        /// <param name="documentAnalysisClient">Azure Document Analysis client</param>
        /// <param name="messageBusService">Service Bus messaging service</param>
        /// <param name="logger">Logger for the service</param>
        public PdfService(
            BlobServiceClient blobServiceClient,
            DocumentAnalysisClient documentAnalysisClient,
            IMessageBusService messageBusService,
            ILogger<PdfService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _documentAnalysisClient = documentAnalysisClient;
            _messageBusService = messageBusService;
            _logger = logger;

            // Ensure container exists
            _blobServiceClient.GetBlobContainerClient(_containerName).CreateIfNotExists();
        }

        /// <summary>
        /// Upload a PDF file to Azure Blob Storage and send a message to Service Bus for processing
        /// </summary>
        /// <param name="pdfStream">Stream containing the PDF content</param>
        /// <param name="fileName">Original file name of the PDF</param>
        /// <returns>Unique blob name for the uploaded PDF</returns>
        public async Task<string> UploadPdfAsync(Stream pdfStream, string fileName)
        {
            try
            {
                // Create a unique name for the file
                string blobName = $"{Guid.NewGuid()}-{fileName}";

                // Get reference to blob container
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Upload file to Blob Storage
                await blobClient.UploadAsync(pdfStream, true);

                // Send a message to Service Bus queue for asynchronous processing
                await _messageBusService.SendMessageAsync(PDF_QUEUE_NAME, new
                {
                    BlobName = blobName,
                    FileName = fileName,
                    UploadTime = DateTime.UtcNow
                });

                _logger.LogInformation("PDF {FileName} uploaded as {BlobName} and message sent to Service Bus", fileName, blobName);

                return blobName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading PDF {FileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Extract text from a PDF file stored in Azure Blob Storage
        /// </summary>
        /// <param name="blobName">Blob name of the PDF file</param>
        /// <returns>List of document chunks containing the extracted text</returns>
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

        /// <summary>
        /// Extract candidate information from a PDF resume
        /// </summary>
        /// <param name="blobName">Blob name of the PDF file</param>
        /// <returns>Candidate object with extracted information</returns>
        public async Task<Candidate> ExtractCandidateInfoFromPdfAsync(string blobName)
        {
            // Extract text chunks from the PDF
            var chunks = await ExtractTextFromPdfAsync(blobName);

            // Combine all text chunks into one string
            var fullText = string.Join("\n", chunks.Select(c => c.Content));

            // Extract information using regex patterns
            var firstName = ExtractValue(fullText, @"First Name:\s*(.+)");
            var lastName = ExtractValue(fullText, @"Last Name:\s*(.+)");
            var email = ExtractValue(fullText, @"Email:\s*([\w\.-]+@[\w\.-]+\.\w+)");
            var phoneNumber = ExtractValue(fullText, @"Phone:\s*(\+?\d[\d\s\-]+)");

            // Create a new Candidate object
            return new Candidate
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Helper method to extract values using regex patterns
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="pattern">Regex pattern with a capture group</param>
        /// <returns>Extracted value or null if not found</returns>
        private string ExtractValue(string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
