using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using ObjectUploadTracking;
using ObjectUploadTracking.Commands;

namespace VehicleSales.Commands;

public interface IConfirmObjectUploadForVehicleSale
{
    Task<Result> Execute(int objectUploadId, int userId, CancellationToken cancellation);
}

internal sealed class ConfirmObjectUploadForVehicleSale(
    VehicleSalesDbContext dbContext,
    IConsumeObjectUpload consumeObjectUpload,
    IAmazonS3 s3Client) : IConfirmObjectUploadForVehicleSale
{
    public async Task<Result> Execute(int objectUploadId, int userId, CancellationToken cancellation) =>
        await consumeObjectUpload.Execute(
            objectUploadId,
            async (objectUpload) =>
            {
                var vehicleSale = await dbContext.VehicleSales.FindAsync([objectUpload.EntityId], cancellation);

                if (vehicleSale is null)
                    return Result.Failure($"Vehicle sale with id {objectUpload.EntityId} not found.");
                if (vehicleSale.SellerId != userId)
                    return Result.Failure($"Object upload with id {objectUploadId} does not belong to user with id {userId}.");

                var objectsNotFound = await GetObjectsNotExistingInDirectory(objectUpload.Directory, objectUpload.ObjectKeys, cancellation);
                if (objectsNotFound.Count > 0)
                    return Result.Failure($"{string.Join(", ", objectsNotFound)} do not exist in {BucketNames.VehicleSales}/{objectUpload.Directory}.");

                vehicleSale.UpdateVehicleDetails(
                    vehicleDetails =>
                    {
                        vehicleDetails.Directory = objectUpload.Directory;
                        vehicleDetails.PhotoKeys = objectUpload.ObjectKeys;
                    });
                await dbContext.SaveChangesAsync(cancellation);

                return Result.Success();
            },
            cancellation);

    private async Task<IReadOnlyList<ObjectKeyName>> GetObjectsNotExistingInDirectory(
        DirectoryName directory,
        IReadOnlyList<ObjectKeyName> objectKeys,
        CancellationToken cancellation)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = BucketNames.VehicleSales,
            Prefix = directory.Value
        };

        var existingKeys = new HashSet<string>();
        var response = await s3Client.ListObjectsV2Async(request, cancellation);
        foreach (var s3Object in response.S3Objects)
            existingKeys.Add(s3Object.Key);

        return [.. objectKeys.Where(key =>
            !existingKeys.Contains($"{directory.Value}/{key.Value}"))];
    }
}
