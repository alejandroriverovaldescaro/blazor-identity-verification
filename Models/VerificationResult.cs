namespace IdentityVerification.Models;

public class VerificationResult
{
    public bool IsVerified { get; set; }
    public double ConfidenceScore { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime AttemptDate { get; set; }
}
