using Microsoft.EntityFrameworkCore;

namespace CarSales.Queries;

public interface IGetCarMakesQuery
{
    Task<IReadOnlyList<string>> Get(CancellationToken cancellationToken);
}

internal class GetCarMakesQuery(
    CarSalesDbContext context) : IGetCarMakesQuery
{
    public async Task<IReadOnlyList<string>> Get(CancellationToken cancellationToken) =>
        await context.CarMakes
            .Select(carMake => carMake.Name)
            .ToArrayAsync(cancellationToken);
}
