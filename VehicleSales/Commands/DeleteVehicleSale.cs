using CSharpFunctionalExtensions;

namespace VehicleSales.Commands;

public interface IDeleteVehicleSale
{
    Task<UnitResult<DeleteVehicleSaleErrorCode>> Execute(int id, int userId, CancellationToken cancellationToken);
}

public enum DeleteVehicleSaleErrorCode
{
    VehicleSaleNotFound,
    UnauthorizedToDelete
}

internal sealed class DeleteVehicleSale(VehicleSalesDbContext dbContext) : IDeleteVehicleSale
{
    private readonly VehicleSalesDbContext _dbContext = dbContext;

    public async Task<UnitResult<DeleteVehicleSaleErrorCode>> Execute(int id, int userId, CancellationToken cancellationToken)
    {
        var vehicleSale = await _dbContext.VehicleSales.FindAsync([id], cancellationToken);
        if (vehicleSale == null)
            return DeleteVehicleSaleErrorCode.VehicleSaleNotFound;

        if (vehicleSale.SellerId != userId)
            return DeleteVehicleSaleErrorCode.UnauthorizedToDelete;

        _dbContext.VehicleSales.Remove(vehicleSale);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<DeleteVehicleSaleErrorCode>();
    }
}
