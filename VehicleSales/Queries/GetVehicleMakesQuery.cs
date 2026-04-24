using Microsoft.EntityFrameworkCore;

namespace VehicleSales.Queries;

public sealed record VehicleMakeDto(int Id, string Name);

public interface IGetVehicleMakesQuery
{
    Task<IReadOnlyList<VehicleMakeDto>> Get(CancellationToken cancellationToken);
}

internal class GetVehicleMakesQuery(
    VehicleSalesDbContext context) : IGetVehicleMakesQuery
{
    public async Task<IReadOnlyList<VehicleMakeDto>> Get(CancellationToken cancellationToken) =>
        await context.VehicleMakes
            .Select(vehicleMake => new VehicleMakeDto(vehicleMake.Id, vehicleMake.Name))
            .ToArrayAsync(cancellationToken);
}
