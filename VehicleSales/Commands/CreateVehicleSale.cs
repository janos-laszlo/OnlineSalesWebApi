using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using ObjectUploadTracking;
using ObjectUploadTracking.Commands;
using VehicleSales.Dtos;

namespace VehicleSales.Commands;

public interface ICreateVehicleSale
{
    Task<Result<ObjectUploadTrackingDto?>> Execute(
        CreateVehicleSaleRequestDto dto,
        int sellerId,
        CancellationToken cancellationToken);
}

internal sealed class CreateVehicleSale(
    VehicleSalesDbContext dbContext,
    ICreateObjectUpload createObjectUpload,
    IAmazonS3 r2Client) : ICreateVehicleSale
{
    public async Task<Result<ObjectUploadTrackingDto?>> Execute(
        CreateVehicleSaleRequestDto dto,
        int sellerId,
        CancellationToken cancellationToken)
    {
        // It's not necessary to check the seller existence here as it's assumed to be valid.

        var vehicleSaleResult = dto.ToVehicleSale(sellerId);
        if (vehicleSaleResult.IsFailure)
            return Result.Failure<ObjectUploadTrackingDto?>(vehicleSaleResult.Error);

        if (!await dbContext.VehicleModels.AnyAsync(
            vehicleModel => vehicleModel.Id == vehicleSaleResult.Value.VehicleDetails.VehicleModelId,
            cancellationToken))
        {
            return Result.Failure<ObjectUploadTrackingDto?>("Vehicle model doesn't exist");
        }

        dbContext.VehicleSales.Add(vehicleSaleResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (dto.PhotoContentTypes?.Any() != true)
            return null;

        return await CreateObjectUpload(
            dto.PhotoContentTypes,
            vehicleSaleResult.Value.Id,
            DateTime.UtcNow.AddMinutes(15),
            cancellationToken);
    }

    private async Task<ObjectUploadTrackingDto> CreateObjectUpload(
        IEnumerable<string> objectContentTypes,
        int entityId,
        DateTime expiresAt,
        CancellationToken cancellation)
    {
        IReadOnlyList<(ObjectKeyName, string)> objKeyAndItsContentType = CreateObjectKeys(objectContentTypes);
        ObjectUpload objectUpload = new()
        {
            EntityId = entityId,
            Directory = DirectoryName.Create(Guid.CreateVersion7().ToString("N")).Value,
            ObjectKeys = [.. objKeyAndItsContentType.Select(tuple => tuple.Item1)],
            ExpiresAt = expiresAt
        };
        await createObjectUpload.Execute(objectUpload, cancellation);

        return new ObjectUploadTrackingDto(
            objectUpload.Id,
            CreatePresignedPutRequests(objectUpload.Directory, objKeyAndItsContentType));
    }

    private static List<(ObjectKeyName, string)> CreateObjectKeys(
        IEnumerable<string> objectContentTypes)
    =>
        [.. objectContentTypes
            .Select((pct, index) => (ObjectKeyName.Create($"{index}.{pct.Split('/')[1]}").Value, pct))];

    private Dictionary<string, string> CreatePresignedPutRequests(
        DirectoryName directory,
        IReadOnlyList<(ObjectKeyName, string)> objectKeysAndTheirContentType)
    =>
        objectKeysAndTheirContentType.ToDictionary(
            objectKey => objectKey.Item1.Value,
            objectKeyAndItsContentType => r2Client.GetPreSignedURL(
                new GetPreSignedUrlRequest
                {
                    BucketName = BucketNames.VehicleSales,
                    Key = $"{directory.Value}/{objectKeyAndItsContentType.Item1.Value}",
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.Now.AddMinutes(15),
                    ContentType = objectKeyAndItsContentType.Item2
                }));
}
