using CustomerAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerAPI.Data;

public sealed class UserProfileDbContext(DbContextOptions<UserProfileDbContext> options)
    : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserProfileDbContext).Assembly);
    }
}
