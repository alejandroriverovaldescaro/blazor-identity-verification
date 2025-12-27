namespace IdentityVerification.Services;

public interface IAzureVisionService
{
    Task<byte[]?> DetectFaceAsync(Stream imageStream);
    Task<double> CompareFacesAsync(Stream image1, Stream image2);
}
