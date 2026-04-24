using Microsoft.EntityFrameworkCore;

namespace VehicleSales.Queries;

public sealed record VehicleModelDto(int Id, string Name);

public interface IGetMakeModelsQuery
{
    Task<IReadOnlyList<VehicleModelDto>> Get(string makeName, CancellationToken cancellationToken);
}

internal sealed class GetMakeModelsQuery(
    VehicleSalesDbContext dbContext) : IGetMakeModelsQuery
{
    public async Task<IReadOnlyList<VehicleModelDto>> Get(string makeName, CancellationToken cancellationToken) =>
        await dbContext.VehicleMakes
            .Where(vehicleMake => vehicleMake.Name == makeName)
            .SelectMany(vehicleMake => vehicleMake.VehicleModels)
            .Select(vehicleModel => new VehicleModelDto(vehicleModel.Id, vehicleModel.Name))
            .ToArrayAsync(cancellationToken);
}
