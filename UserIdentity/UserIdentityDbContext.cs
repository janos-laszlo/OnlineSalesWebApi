using Microsoft.EntityFrameworkCore;

namespace UserIdentity;

public sealed class UserIdentityDbContext(
    DbContextOptions<UserIdentityDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserIdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}