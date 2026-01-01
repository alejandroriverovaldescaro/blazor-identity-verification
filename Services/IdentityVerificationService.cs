using IdentityVerification.Data;
using IdentityVerification.Models;

namespace IdentityVerification.Services;

public class IdentityVerificationService : IIdentityVerificationService
{
    private readonly IAzureVisionService _visionService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<IdentityVerificationService> _logger;

    public IdentityVerificationService(
        IAzureVisionService visionService,
        IBlobStorageService blobStorageService,
        ApplicationDbContext dbContext,
        ILogger<IdentityVerificationService> logger)
    {
        _visionService = visionService;
        _blobStorageService = blobStorageService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<VerificationResult> VerifyIdentityAsync(Stream documentImage, Stream selfieImage)
    {
        try
        {
            _logger.LogInformation("Starting identity verification process");

            // Detect faces in both images
            var documentFace = await _visionService.DetectFaceAsync(documentImage);
            var selfieFace = await _visionService.DetectFaceAsync(selfieImage);

            if (documentFace == null || selfieFace == null)
            {
                return new VerificationResult
                {
                    IsVerified = false,
                    ConfidenceScore = 0.0,
                    Message = "Could not detect face in one or both images",
                    AttemptDate = DateTime.UtcNow
                };
            }

            // Compare faces
            documentImage.Position = 0;
            selfieImage.Position = 0;
            var confidenceScore = await _visionService.CompareFacesAsync(documentImage, selfieImage);

            // Determine if verification passes (threshold: 70%)
            const double threshold = 0.70;
            var isVerified = confidenceScore >= threshold;

            return new VerificationResult
            {
                IsVerified = isVerified,
                ConfidenceScore = confidenceScore,
                Message = isVerified 
                    ? $"Identity verified successfully with {confidenceScore:P2} confidence" 
                    : $"Identity verification failed. Confidence score {confidenceScore:P2} is below threshold of {threshold:P2}",
                AttemptDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during identity verification");
            return new VerificationResult
            {
                IsVerified = false,
                ConfidenceScore = 0.0,
                Message = $"Verification failed due to error: {ex.Message}",
                AttemptDate = DateTime.UtcNow
            };
        }
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName)
    {
        try
        {
            var blobUrl = await _blobStorageService.UploadImageAsync(imageStream, fileName);
            _logger.LogInformation($"Image saved successfully: {fileName}");
            return blobUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving image: {fileName}");
            throw;
        }
    }

    public async Task<VerificationAttempt> SaveVerificationAttemptAsync(
        VerificationResult result, 
        string documentPath, 
        string selfiePath)
    {
        try
        {
            var attempt = new VerificationAttempt
            {
                DocumentPath = documentPath,
                SelfiePath = selfiePath,
                IsVerified = result.IsVerified,
                ConfidenceScore = result.ConfidenceScore,
                AttemptDate = result.AttemptDate
            };

            _dbContext.VerificationAttempts.Add(attempt);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Verification attempt saved with ID: {attempt.Id}");
            
            return attempt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving verification attempt");
            throw;
        }
    }
}
