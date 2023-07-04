﻿using Azure.Storage.Blobs;
using BulkyBook.Models.StorageModels;
using Microsoft.AspNetCore.Http;

namespace BulkyBook.CloudStorage.Service
{
    public interface IAzureStorage
    {
        /// <summary>
        /// This method uploads a file submitted with the request
        /// </summary>
        /// <param name="file">File for upload</param>
        /// <returns>Blob with status</returns>
        Task<BlobResponseDto> UploadAsync(IFormFile file);

        /// <summary>
        /// This method deleted a file with the specified filename
        /// </summary>
        /// <param name="blobFilename">Filename</param>
        /// <returns>Blob with status</returns>
        Task<BlobResponseDto> DeleteAsync(string blobFilename);

        Task<bool> ImageExists(string blobFilename);


        Task<BlobClient> GenerateSASToken(string blobFileName);
    }
}