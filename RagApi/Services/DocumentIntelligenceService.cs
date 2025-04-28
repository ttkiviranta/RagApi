using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    /// <summary>
    /// Service for extracting text from documents using Azure Document Intelligence
    /// </summary>
    public class DocumentIntelligenceService : IDocumentIntelligenceService
    {
        private readonly DocumentAnalysisClient _documentClient;

        public DocumentIntelligenceService(DocumentAnalysisClient documentClient)
        {
            _documentClient = documentClient;
        }

        /// <inheritdoc/>
        public async Task<DocumentAnalysisResult> ExtractTextFromPdfAsync(byte[] pdfContent)
        {
            try
            {
                // Use document analysis to extract text from PDF
                using var stream = new MemoryStream(pdfContent);
                AnalyzeDocumentOperation operation = await _documentClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", stream);

                // Get the operation result
                AnalyzeResult analyzeResult = operation.Value;

                // Process the result into our model
                var result = new DocumentAnalysisResult
                {
                    Content = string.Empty,
                    Pages = new System.Collections.Generic.List<RagApi.Models.DocumentPage>()
                };

                var contentBuilder = new System.Text.StringBuilder();

                // Extract text from each page
                foreach (var page in analyzeResult.Pages)
                {
                    var pageContent = new System.Text.StringBuilder();

                    // Extract content from each line on the page
                    foreach (var line in page.Lines)
                    {
                        pageContent.AppendLine(line.Content);
                    }

                    // Add page to result - use fully qualified name to avoid ambiguity
                    result.Pages.Add(new RagApi.Models.DocumentPage
                    {
                        PageNumber = (int)page.PageNumber,
                        Content = pageContent.ToString()
                    });

                    // Add to overall content
                    contentBuilder.AppendLine(pageContent.ToString());
                    contentBuilder.AppendLine();
                }

                result.Content = contentBuilder.ToString();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting text from PDF: {ex.Message}", ex);
            }
        }
    }
}