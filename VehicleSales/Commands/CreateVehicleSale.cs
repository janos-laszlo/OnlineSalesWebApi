using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using VehicleSales.Dtos;
using VehicleSales.Entities.VehicleSale;

namespace VehicleSales.Commands;

public interface ICreateVehicleSale
{
    Task<Result<int>> Execute(
        CreateVehicleSaleRequestDto dto,
        int sellerId,
        CancellationToken cancellationToken);
}

internal sealed class CreateVehicleSale(
    VehicleSalesDbContext dbContext,
    IAmazonS3 r2Client,
    IConfiguration configuration) : ICreateVehicleSale
{
    public async Task<Result<int>> Execute(
        CreateVehicleSaleRequestDto dto,
        int sellerId,
        CancellationToken cancellationToken)
    {
        var vehicleSaleResult = dto.ToVehicleSale(sellerId);
        if (vehicleSaleResult.IsFailure)
            return Result.Failure<int>(vehicleSaleResult.Error);

        dbContext.VehicleSales.Add(vehicleSaleResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (dto.Photos is { Count: > 0 })
            await UploadPhotos(vehicleSaleResult.Value, dto.Photos, cancellationToken);

        return vehicleSaleResult.Value.Id;
    }

    private async Task UploadPhotos(
        VehicleSale vehicleSale,
        IFormFileCollection photos,
        CancellationToken cancellation)
    {
        var bucketName = configuration[R2Config.BucketNameKey] ??
            throw new InvalidOperationException("Bucket name not set in configuration");

        var directory = DirectoryName.Create(Guid.CreateVersion7().ToString("N")).Value;
        var photoKeys = new List<ObjectKeyName>(photos.Count);

        for (int i = 0; i < photos.Count; i++)
        {
            var photo = photos[i];
            var extension = photo.ContentType.Split('/')[1];
            var key = ObjectKeyName.Create($"{i}.{extension}").Value;

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = $"{directory.Value}/{key.Value}",
                InputStream = photo.OpenReadStream(),
                ContentType = photo.ContentType,
                DisablePayloadSigning = true
            };

            await r2Client.PutObjectAsync(putRequest, cancellation);
            photoKeys.Add(key);
        }

        vehicleSale.UpdateVehicleDetails(vd =>
        {
            vd.Directory = directory;
            vd.PhotoKeys = photoKeys;
        });

        await dbContext.SaveChangesAsync(cancellation);
    }
}
