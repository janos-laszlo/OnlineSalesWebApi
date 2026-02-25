namespace VehicleSales.Commands;

public interface IRefreshPhotoUploadUrls
{
    Task<Result<IReadOnlyList<PhotoUploadSlot>>> Execute(
        int saleId, int sellerId, CancellationToken cancellationToken);
}

internal sealed class RefreshPhotoUploadUrls(
    VehicleSalesDbContext dbContext,
    IStorageService storage) : IRefreshPhotoUploadUrls
{
    private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(15);

    public async Task<Result<IReadOnlyList<PhotoUploadSlot>>> Execute(
        int saleId, int sellerId, CancellationToken cancellationToken)
    {
        var sale = await dbContext.VehicleSales
            .Include(s => s.Photos)
            .FirstOrDefaultAsync(s => s.Id == saleId, cancellationToken);

        if (sale is null) return Result.Failure<IReadOnlyList<PhotoUploadSlot>>("Sale not found");
        if (sale.SellerId != sellerId) return Result.Failure<IReadOnlyList<PhotoUploadSlot>>("Forbidden");
        if (sale.Status != VehicleSaleStatus.Draft)
            return Result.Failure<IReadOnlyList<PhotoUploadSlot>>("Sale is not in Draft status");

        var expiresAt = DateTimeOffset.UtcNow.Add(Expiry);
        var slots = new List<PhotoUploadSlot>();

        foreach (var photo in sale.Photos.OrderBy(p => p.DisplayOrder))
        {
            // Only refresh slots that haven't been uploaded yet
            if (await storage.ObjectExistsAsync(photo.ObjectKey))
                continue;

            var url = await storage.GenerateUploadUrlAsync(photo.ObjectKey, Expiry);
            slots.Add(new PhotoUploadSlot(photo.DisplayOrder, photo.ObjectKey, url, expiresAt));
        }

        return slots;
    }
}