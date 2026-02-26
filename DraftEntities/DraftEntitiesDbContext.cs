using Microsoft.EntityFrameworkCore;

namespace DraftEntities;

internal sealed class DraftEntitiesDbContext(
    DbContextOptions<DraftEntitiesDbContext> options) : DbContext(options)
{
    internal DbSet<DraftEntity> DraftEntities { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DraftEntitiesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
