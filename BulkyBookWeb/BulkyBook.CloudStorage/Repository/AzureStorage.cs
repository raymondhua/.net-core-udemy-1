using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BulkyBook.CloudStorage.Service;
using BulkyBook.Models.StorageModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Numerics;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using BulkyBook.Models;

namespace BulkyBook.CloudStorage.Repository
{
    public class AzureStorage : IAzureStorage
    {
        #region Dependency Injection / Constructor

        private readonly StorageSettings _storageConfig;
        private readonly ILogger<AzureStorage> _logger;

        public AzureStorage(IConfiguration configuration, ILogger<AzureStorage> logger)
        {
            _storageConfig = configuration.GetSection("BlobStorageSettings").Get<StorageSettings>();
            _logger = logger;
        }

        #endregion


        public async Task<BlobResponseDto> UploadAsync(IFormFile blob, bool useGuid = true)
        {
            // Create new upload response object that we can return to the requesting method
            BlobResponseDto response = new();

            // Get a reference to a container named in appsettings.json and then create it
            BlobContainerClient container =
                new BlobContainerClient(_storageConfig.ConnectionString, _storageConfig.ContainerName);
            //await container.CreateAsync();
            try
            {
                string fileName;
                if (useGuid)
                    fileName = Guid.NewGuid() + Path.GetExtension(blob.FileName);
                else
                    fileName = blob.FileName;

                // Get a reference to the blob just uploaded from the API in a container from configuration settings
                BlobClient client = container.GetBlobClient(fileName);

                // Open a stream for the file we want to upload
                await using (Stream? data = blob.OpenReadStream())
                {
                    // Upload the file async
                    await client.UploadAsync(data);
                }

                // Everything is OK and file got uploaded
                response.Status = $"File {blob.FileName} Uploaded Successfully";
                response.Error = false;
                response.Blob.Uri = client.Uri.AbsoluteUri;
                response.Blob.Name = client.Name;

            }
            // If the file already exists, we catch the exception and do not upload it
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                _logger.LogError(
                    $"File with name {blob.FileName} already exists in container. Set another name to store the file in the container: '{_storageConfig.ContainerName}.'");
                response.Status =
                    $"File with name {blob.FileName} already exists. Please use another name to store your file.";
                response.Error = true;
                return response;
            }
            // If we get an unexpected error, we catch it here and return the error message
            catch (RequestFailedException ex)
            {
                // Log error to console and create a new response we can return to the requesting method
                _logger.LogError($"Unhandled Exception. ID: {ex.StackTrace} - Message: {ex.Message}");
                response.Status = $"Unexpected error: {ex.StackTrace}. Check log with StackTrace ID.";
                response.Error = true;
                return response;
            }

            // Return the BlobUploadResponse object
            return response;
        }

        public async Task<BlobDto> GetAsync(string blobFilename)
        {
            // Get a reference to a container named in appsettings.json
            BlobContainerClient client =
                new BlobContainerClient(_storageConfig.ConnectionString, _storageConfig.ContainerName);

            try
            {
                // Get a reference to the blob uploaded earlier from the API in the container from configuration settings
                BlobClient file = client.GetBlobClient(blobFilename);

                // Check if the file exists in the container
                if (await file.ExistsAsync())
                {
                    var data = await file.OpenReadAsync();
                    Stream blobContent = data;

                    // Download the file details async
                    var content = await file.DownloadContentAsync();

                    // Add data to variables in order to return a BlobDto
                    string name = blobFilename;
                    string contentType = content.Value.Details.ContentType;

                    // Create new BlobDto with blob data from variables
                    return new BlobDto { Content = blobContent, Name = name, ContentType = contentType };
                }
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // Log error to console
                _logger.LogError($"File {blobFilename} was not found.");
            }

            // File does not exist, return null and handle that in requesting method
            return null;
        }

        public async Task<bool> ImageExists(string blobFilename)
        {
            // Get a reference to a container named in appsettings.json
            BlobContainerClient client =
                new BlobContainerClient(_storageConfig.ConnectionString, _storageConfig.ContainerName);

            try
            {
                // Get a reference to the blob uploaded earlier from the API in the container from configuration settings
                BlobClient file = client.GetBlobClient(blobFilename);

                // Check if the file exists in the container
                if (await file.ExistsAsync())
                    return true;

            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // Log error to console
                _logger.LogError($"File {blobFilename} was not found.");
            }

            // File does not exist, return null and handle that in requesting method
            return false;
        }

        public async Task<BlobResponseDto> DeleteAsync(string blobFilename)
        {
            BlobContainerClient client =
                new BlobContainerClient(_storageConfig.ConnectionString, _storageConfig.ContainerName);

            BlobClient file = client.GetBlobClient(blobFilename);

            try
            {
                // Delete the file
                await file.DeleteAsync();
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // File did not exist, log to console and return new response to requesting method
                _logger.LogError($"File {blobFilename} was not found.");
                return new BlobResponseDto { Error = true, Status = $"File with name {blobFilename} not found." };
            }

            // Return a new BlobResponseDto to the requesting method
            return new BlobResponseDto
                { Error = false, Status = $"File: {blobFilename} has been successfully deleted." };

        }

        public async Task<BlobClient> GenerateSasToken()
        {

            BlobContainerClient client =
                new BlobContainerClient(_storageConfig.ConnectionString, _storageConfig.ContainerName);

            Uri blobSasURI = await CreateServiceSasBlob(client);
            return new BlobClient(blobSasURI);
        }

        public Uri GenerateSasUri() =>  Task.Run(async () => await GenerateSasToken()).Result.Uri;

        public string GetSasToken() => GenerateSasUri().Query;

        public string AppendSasTokenToUrl(string fileName) => fileName += GetSasToken();

        public static async Task<Uri> CreateServiceSasBlob(
            BlobContainerClient blobClient,
            string storedPolicyName = null)
        {
            // Check if BlobContainerClient object has been authorized with Shared Key
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one day
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(1);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
                }
                else
                    sasBuilder.Identifier = storedPolicyName;

                Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

                return sasURI;
            }
            // Client object is not authorized via Shared Key
            return null;
        }

        public string GenerateUrlWithSasToken(string fileName)
        {
            var uri = GenerateSasUri();
            string url = "https://" + uri.Host + uri.AbsolutePath + "/" + fileName +
                         uri.Query;
            return url;
        }
    }
}

