using Microsoft.EntityFrameworkCore;

namespace VehicleSales.Queries;

public interface IGetVehicleMakesQuery
{
    Task<IReadOnlyList<string>> Get(CancellationToken cancellationToken);
}

internal class GetVehicleMakesQuery(
    VehicleSalesDbContext context) : IGetVehicleMakesQuery
{
    public async Task<IReadOnlyList<string>> Get(CancellationToken cancellationToken) =>
        await context.VehicleMakes
            .Select(vehicleMake => vehicleMake.Name)
            .ToArrayAsync(cancellationToken);
}
