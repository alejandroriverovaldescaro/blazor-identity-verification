namespace IdentityVerification.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);
    Task<Stream> DownloadImageAsync(string fileName);
    Task DeleteImageAsync(string fileName);
}
