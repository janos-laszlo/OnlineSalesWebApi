using Microsoft.EntityFrameworkCore;
using UserIdentity.Entities;

namespace UserIdentity;

internal sealed class UserIdentityDbContext(
    DbContextOptions<UserIdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Email> Emails { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserIdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}