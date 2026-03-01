using Microsoft.EntityFrameworkCore;

namespace ObjectUploadTracking;

internal sealed class ObjectUploadTrackingDbContext(
    DbContextOptions<ObjectUploadTrackingDbContext> options) : DbContext(options)
{
    internal DbSet<ObjectUpload> ObjectUploads { get; private set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ObjectUploadTrackingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
