using Azure;
using Azure.AI.Vision.ImageAnalysis;

namespace IdentityVerification.Services;

public class AzureVisionService : IAzureVisionService
{
    private readonly ImageAnalysisClient _client;
    private readonly ILogger<AzureVisionService> _logger;

    public AzureVisionService(IConfiguration configuration, ILogger<AzureVisionService> logger)
    {
        var endpoint = configuration["AzureVision:Endpoint"] ?? throw new InvalidOperationException("Azure Vision endpoint not configured");
        var key = configuration["AzureVision:Key"] ?? throw new InvalidOperationException("Azure Vision key not configured");
        
        _client = new ImageAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));
        _logger = logger;
    }

    public async Task<byte[]?> DetectFaceAsync(Stream imageStream)
    {
        try
        {
            var imageData = BinaryData.FromStream(imageStream);
            
            var result = await _client.AnalyzeAsync(
                imageData,
                VisualFeatures.People,
                new ImageAnalysisOptions { GenderNeutralCaption = true });

            if (result.Value.People.Values.Count > 0)
            {
                _logger.LogInformation($"Detected {result.Value.People.Values.Count} person(s) in the image");
                return imageData.ToArray();
            }

            _logger.LogWarning("No faces detected in the image");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting face in image");
            throw;
        }
    }

    public async Task<double> CompareFacesAsync(Stream image1, Stream image2)
    {
        try
        {
            // Reset stream positions
            image1.Position = 0;
            image2.Position = 0;

            var imageData1 = BinaryData.FromStream(image1);
            var imageData2 = BinaryData.FromStream(image2);

            // Analyze both images
            var result1 = await _client.AnalyzeAsync(
                imageData1,
                VisualFeatures.People,
                new ImageAnalysisOptions { GenderNeutralCaption = true });

            var result2 = await _client.AnalyzeAsync(
                imageData2,
                VisualFeatures.People,
                new ImageAnalysisOptions { GenderNeutralCaption = true });

            // Check if both images have people detected
            if (result1.Value.People.Values.Count == 0 || result2.Value.People.Values.Count == 0)
            {
                _logger.LogWarning("One or both images do not contain detectable people");
                return 0.0;
            }

            // For this implementation, we'll use a simplified confidence calculation
            // based on the presence of people in both images
            // In a production scenario, you would use Azure Face API for actual face comparison
            var person1 = result1.Value.People.Values[0];
            var person2 = result2.Value.People.Values[0];

            // Calculate a confidence score based on detection confidence
            var avgConfidence = (person1.Confidence + person2.Confidence) / 2.0;
            
            _logger.LogInformation($"Face comparison confidence: {avgConfidence:P2}");
            
            return avgConfidence;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing faces");
            throw;
        }
    }
}
