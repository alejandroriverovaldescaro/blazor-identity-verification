using Microsoft.EntityFrameworkCore;
using IdentityVerification.Models;

namespace IdentityVerification.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<VerificationAttempt> VerificationAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VerificationAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentPath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.SelfiePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsVerified).IsRequired();
            entity.Property(e => e.ConfidenceScore).IsRequired();
            entity.Property(e => e.AttemptDate).IsRequired();
        });
    }
}
