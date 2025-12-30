using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace IdentityVerification.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<BlobStorageService> _logger;

    private bool _containerEnsured = false;
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"] 
            ?? throw new InvalidOperationException("Azure Storage connection string not configured");
        
        _containerName = configuration["AzureStorage:ContainerName"] ?? "verification-images";
        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;
        
        _logger.LogInformation("BlobStorageService initialized (container will be ensured on first use)");
    }

    private async Task EnsureContainerExistsAsync()
    {
        if (_containerEnsured)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_containerEnsured)
                return;

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            _logger.LogInformation($"Container '{_containerName}' is ready");
            _containerEnsured = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error ensuring container '{_containerName}' exists");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        await EnsureContainerExistsAsync();
        
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            imageStream.Position = 0;
            
            await blobClient.UploadAsync(imageStream, overwrite: true);
            
            _logger.LogInformation($"Uploaded image: {fileName}");
            
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading image: {fileName}");
            throw;
        }
    }

    public async Task<Stream> DownloadImageAsync(string fileName)
    {
        await EnsureContainerExistsAsync();
        
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DownloadAsync();
            
            var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            _logger.LogInformation($"Downloaded image: {fileName}");
            
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading image: {fileName}");
            throw;
        }
    }

    public async Task DeleteImageAsync(string fileName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
            
            _logger.LogInformation($"Deleted image: {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting image: {fileName}");
            throw;
        }
    }
}
