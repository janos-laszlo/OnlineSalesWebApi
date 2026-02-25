using Microsoft.EntityFrameworkCore;
using VehicleSales.Entities.User;
using VehicleSales.Entities.VehicleMake;

namespace VehicleSales;

internal sealed class VehicleSalesDbContext(
    DbContextOptions<VehicleSalesDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Use as readonly because this belongs to the UserIdentity context.
    /// </summary>
    internal DbSet<User> UsersReadOnly { get; private set; }
    internal DbSet<VehicleMake> VehicleMakes { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehicleSalesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
