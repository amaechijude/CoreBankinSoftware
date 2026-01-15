using CustomerProfile.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerProfile.Data;

public sealed class UserProfileDbContext(DbContextOptions<UserProfileDbContext> options)
    : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();
    public DbSet<SetPasswordSession> SetPasswordSessions => Set<SetPasswordSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserProfileDbContext).Assembly);
    }
}
