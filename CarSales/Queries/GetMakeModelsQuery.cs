using Microsoft.EntityFrameworkCore;

namespace CarSales.Queries;

public interface IGetMakeModelsQuery
{
    Task<IReadOnlyList<string>> Get(string makeName, CancellationToken cancellationToken);
}

internal sealed class GetMakeModelsQuery(
    CarSalesDbContext dbContext) : IGetMakeModelsQuery
{
    public async Task<IReadOnlyList<string>> Get(string makeName, CancellationToken cancellationToken) =>
        await dbContext.CarMakes
            .Where(carMake => carMake.Name == makeName)
            .SelectMany(carMake => carMake.CarModels)
            .Select(carModel => carModel.Name)
            .ToArrayAsync(cancellationToken);
}
