using CarSales.Entities.CarMake;
using CarSales.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace CarSales;

internal sealed class CarSalesDbContext(
    DbContextOptions<CarSalesDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Use as readonly because this belongs to the UserIdentity context.
    /// </summary>
    internal DbSet<User> UsersReadOnly { get; private set; }
    internal DbSet<CarMake> CarMakes { get; private set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarSalesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
