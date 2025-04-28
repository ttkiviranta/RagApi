using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Interface for Azure Document Intelligence service operations
    /// </summary>
    public interface IDocumentIntelligenceService
    {
        /// <summary>
        /// Extracts text content from a PDF document
        /// </summary>
        Task<DocumentAnalysisResult> ExtractTextFromPdfAsync(byte[] pdfContent);
    }
}
