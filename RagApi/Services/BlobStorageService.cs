using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RagApi.Interfaces;

namespace RagApi.Services
{
    /// <summary>
    /// Service for Azure Blob Storage operations
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "documents"; // Default container name

        public BlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;

            // Ensure container exists
            _ = EnsureContainerExistsAsync();
        }

        /// <inheritdoc/>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var containerClient = await GetContainerClientAsync();
                var blobClient = containerClient.GetBlobClient(fileName);

                // Upload file with metadata
                var blobUploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };

                await blobClient.UploadAsync(fileStream, blobUploadOptions);

                // Return the blob path (can be used to download later)
                return blobClient.Name;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file to blob storage: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> DownloadFileAsync(string blobPath)
        {
            try
            {
                var containerClient = await GetContainerClientAsync();
                var blobClient = containerClient.GetBlobClient(blobPath);

                // Download blob content
                BlobDownloadInfo download = await blobClient.DownloadAsync();

                using var memoryStream = new MemoryStream();
                await download.Content.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file from blob storage: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> DownloadFileAsTextAsync(string blobPath)
        {
            try
            {
                byte[] fileBytes = await DownloadFileAsync(blobPath);
                return Encoding.UTF8.GetString(fileBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file as text: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFileAsync(string blobPath)
        {
            try
            {
                var containerClient = await GetContainerClientAsync();
                var blobClient = containerClient.GetBlobClient(blobPath);

                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file from blob storage: {ex.Message}", ex);
            }
        }

        private async Task<BlobContainerClient> GetContainerClientAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await EnsureContainerExistsAsync();
            return containerClient;
        }

        private async Task EnsureContainerExistsAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
        }
    }
}