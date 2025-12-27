using IdentityVerification.Models;

namespace IdentityVerification.Services;

public interface IIdentityVerificationService
{
    Task<VerificationResult> VerifyIdentityAsync(Stream documentImage, Stream selfieImage);
    Task<string> SaveImageAsync(Stream imageStream, string fileName);
    Task<VerificationAttempt> SaveVerificationAttemptAsync(VerificationResult result, string documentPath, string selfiePath);
}
