using CSharpFunctionalExtensions;

namespace VehicleSales.Commands;

public interface IDeleteVehicleSale
{
    Task<Result> Execute(int id, int userId, CancellationToken cancellationToken);
}

internal sealed class DeleteVehicleSale(VehicleSalesDbContext dbContext) : IDeleteVehicleSale
{
    private readonly VehicleSalesDbContext _dbContext = dbContext;

    public async Task<Result> Execute(int id, int userId, CancellationToken cancellationToken)
    {
        var vehicleSale = await _dbContext.VehicleSales.FindAsync(id, cancellationToken);
        if (vehicleSale == null)
            return Result.Failure("Vehicle sale not found");

        if (vehicleSale.SellerId != userId)
            return Result.Failure("Unauthorized to delete this vehicle sale");

        _dbContext.VehicleSales.Remove(vehicleSale);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
