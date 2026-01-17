using Microsoft.EntityFrameworkCore;

namespace UserIdentity;

internal sealed class UserIdentityDbContext(
    DbContextOptions<UserIdentityDbContext> options) : DbContext(options)
{
    public DbSet<Entities.User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserIdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}