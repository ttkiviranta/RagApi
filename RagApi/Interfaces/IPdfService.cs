using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Service for processing PDF documents
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Uploads a PDF file to storage
        /// </summary>
        /// <param name="pdfStream">The PDF file stream</param>
        /// <param name="fileName">Original filename</param>
        /// <returns>Unique blob name/identifier</returns>
        Task<string> UploadPdfAsync(Stream pdfStream, string fileName);

        /// <summary>
        /// Extracts text content from a PDF file
        /// </summary>
        /// <param name="blobName">The blob identifier</param>
        /// <returns>List of text chunks from the document</returns>
        Task<List<DocumentChunk>> ExtractTextFromPdfAsync(string blobName);
    }
}
