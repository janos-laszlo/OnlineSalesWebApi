using Microsoft.EntityFrameworkCore;

namespace VehicleSales.Queries;

public interface IGetMakeModelsQuery
{
    Task<IReadOnlyList<string>> Get(string makeName, CancellationToken cancellationToken);
}

internal sealed class GetMakeModelsQuery(
    VehicleSalesDbContext dbContext) : IGetMakeModelsQuery
{
    public async Task<IReadOnlyList<string>> Get(string makeName, CancellationToken cancellationToken) =>
        await dbContext.VehicleMakes
            .Where(vehicleMake => vehicleMake.Name == makeName)
            .SelectMany(vehicleMake => vehicleMake.VehicleModels)
            .Select(vehicleModel => vehicleModel.Name)
            .ToArrayAsync(cancellationToken);
}
