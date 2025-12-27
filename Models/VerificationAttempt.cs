using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityVerification.Models;

public class VerificationAttempt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string DocumentPath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string SelfiePath { get; set; } = string.Empty;
    
    public bool IsVerified { get; set; }
    
    public double ConfidenceScore { get; set; }
    
    public DateTime AttemptDate { get; set; }
}
