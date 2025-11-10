using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Interface for Azure Blob Storage operations
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads a file to blob storage
        /// </summary>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Downloads a file from blob storage as byte array
        /// </summary>
        Task<byte[]> DownloadFileAsync(string blobPath);

        /// <summary>
        /// Downloads a file from blob storage as text
        /// </summary>
        Task<string> DownloadFileAsTextAsync(string blobPath);

        /// <summary>
        /// Deletes a file from blob storage
        /// </summary>
        Task DeleteFileAsync(string blobPath);
    }
}
