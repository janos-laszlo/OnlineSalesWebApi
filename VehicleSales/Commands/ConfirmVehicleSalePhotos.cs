namespace VehicleSales.Commands;

public interface IConfirmVehicleSalePhotos
{
    Task<Result> Execute(int saleId, int sellerId, CancellationToken cancellationToken);
}

internal sealed class ConfirmVehicleSalePhotos(
    VehicleSalesDbContext dbContext,
    IStorageService storage) : IConfirmVehicleSalePhotos
{
    public async Task<Result> Execute(
        int saleId, int sellerId, CancellationToken cancellationToken)
    {
        var sale = await dbContext.VehicleSales
            .Include(s => s.Photos)
            .FirstOrDefaultAsync(s => s.Id == saleId, cancellationToken);

        if (sale is null)
            return Result.Failure("Sale not found");

        if (sale.SellerId != sellerId)
            return Result.Failure("Forbidden");

        if (sale.Status != VehicleSaleStatus.Draft)
            return Result.Failure("Sale is not awaiting photo confirmation");

        // Verify every expected photo was actually uploaded
        var missingKeys = new List<string>();
        foreach (var photo in sale.Photos)
        {
            if (!await storage.ObjectExistsAsync(photo.ObjectKey))
                missingKeys.Add(photo.ObjectKey);
        }

        if (missingKeys.Count > 0)
            return Result.Failure(
                $"The following photos were not uploaded: {string.Join(", ", missingKeys)}");

        sale.Status = VehicleSaleStatus.Active;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}