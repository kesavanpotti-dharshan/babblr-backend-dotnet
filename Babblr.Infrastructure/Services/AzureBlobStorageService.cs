using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Babblr.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Babblr.Infrastructure.Services;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IConfiguration config)
    {
        var connectionString = config["Azure:BlobStorageConnection"]!;
        var containerName = config["Azure:BlobContainerName"]!;

        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream, string fileName, string contentType)
    {
        // Ensures container exists before every upload — cheap no-op if already exists
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var blobClient = _containerClient.GetBlobClient(uniqueFileName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
        {
            ContentType = contentType
        });

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
    }
}