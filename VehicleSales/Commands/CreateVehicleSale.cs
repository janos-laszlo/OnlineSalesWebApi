using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using ObjectUploadTracking;
using ObjectUploadTracking.Commands;
using VehicleSales.Dtos;

namespace VehicleSales.Commands;

public interface ICreateVehicleSale
{
    Task<Result<ObjectUploadTrackingDto>> Execute(
        CreateVehicleSaleRequestDto dto,
        int sellerId,
        CancellationToken cancellationToken);
}

internal sealed class CreateVehicleSale(
    VehicleSalesDbContext dbContext,
    ICreateObjectUpload createObjectUpload,
    IAmazonS3 r2Client,
    IConfiguration configuration) : ICreateVehicleSale
{
    public async Task<Result<ObjectUploadTrackingDto>> Execute(
        CreateVehicleSaleRequestDto dto,
        int sellerId,
        CancellationToken cancellationToken)
    {
        // It's not necessary to check the seller existence here as it's assumed to be valid.

        var vehicleSaleResult = dto.ToVehicleSale(sellerId);
        if (vehicleSaleResult.IsFailure)
            return Result.Failure<ObjectUploadTrackingDto>(vehicleSaleResult.Error);

        dbContext.VehicleSales.Add(vehicleSaleResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (dto.PhotoContentTypes?.Any() != true)
            return new ObjectUploadTrackingDto(vehicleSaleResult.Value.Id);

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
            Module = Constants.ModuleName,
            EntityId = entityId,
            Directory = DirectoryName.Create(Guid.CreateVersion7().ToString("N")).Value,
            ObjectKeys = [.. objKeyAndItsContentType.Select(tuple => tuple.Item1)],
            ExpiresAt = expiresAt
        };
        await createObjectUpload.Execute(objectUpload, cancellation);

        return new ObjectUploadTrackingDto(entityId)
        {
            ObjectUploadId = objectUpload.Id,
            ObjectKeysAndTheirPresignedUploadUrls = CreatePresignedPutRequests(
                objectUpload.Directory, objKeyAndItsContentType)
        };
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
                    BucketName = configuration[R2Config.BucketNameKey],
                    Key = $"{directory.Value}/{objectKeyAndItsContentType.Item1.Value}",
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.Now.AddMinutes(15),
                    ContentType = objectKeyAndItsContentType.Item2
                }));
}
