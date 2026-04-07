using Amazon.S3;
using Amazon.S3.Model;
using Common;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using ObjectUploadTracking;
using ObjectUploadTracking.Commands;

namespace VehicleSales.Commands;

public interface IConfirmObjectUploadForVehicleSale
{
    Task<UnitResult<ObjectUploadErrorCode>> Execute(int objectUploadId, int userId, CancellationToken cancellation);
}

internal sealed class ConfirmObjectUploadForVehicleSale(
    VehicleSalesDbContext dbContext,
    IConsumeObjectUpload consumeObjectUpload,
    IAmazonS3 s3Client,
    IConfiguration configuration) : IConfirmObjectUploadForVehicleSale
{
    public async Task<UnitResult<ObjectUploadErrorCode>> Execute(int objectUploadId, int userId, CancellationToken cancellation) =>
        await consumeObjectUpload.Execute(
            objectUploadId,
            async (objectUpload) =>
            {
                var vehicleSale = await dbContext.VehicleSales.FindAsync([objectUpload.EntityId], cancellation);

                if (vehicleSale is null)
                    return ObjectUploadErrorCode.EntityNotFound;
                if (vehicleSale.SellerId != userId)
                    return ObjectUploadErrorCode.ObjectUploadDoesNotBelongToUser;

                var objectsNotFound = await GetObjectsNotExistingInDirectory(objectUpload.Directory, objectUpload.ObjectKeys, cancellation);

                vehicleSale.UpdateVehicleDetails(
                    vehicleDetails =>
                    {
                        vehicleDetails.Directory = objectUpload.Directory;
                        vehicleDetails.PhotoKeys = [.. objectUpload.ObjectKeys.Except(objectsNotFound)];
                    });
                await dbContext.SaveChangesAsync(cancellation);

                return objectsNotFound.Count > 0
                    ? ObjectUploadErrorCode.ObjectUploadNotFound
                    : UnitResult.Success<ObjectUploadErrorCode>();
            },
            cancellation);

    private async Task<IReadOnlyList<ObjectKeyName>> GetObjectsNotExistingInDirectory(
        DirectoryName directory,
        IReadOnlyList<ObjectKeyName> objectKeys,
        CancellationToken cancellation)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = configuration[R2Config.BucketNameKey],
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
