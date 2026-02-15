using CarSales.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarSales;

internal sealed class CarSalesDbContext(
    DbContextOptions<CarSalesDbContext> options) : DbContext(options)
{
    internal DbSet<User> Users { get; private set; }
}
