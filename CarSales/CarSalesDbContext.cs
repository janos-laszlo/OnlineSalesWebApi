using CarSales.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarSales;

internal sealed class CarSalesDbContext(
    DbContextOptions<CarSalesDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; private set; }
}
