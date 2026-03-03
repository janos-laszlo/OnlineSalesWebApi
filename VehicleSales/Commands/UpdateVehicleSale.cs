using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ObjectUploadTracking;
using ObjectUploadTracking.Commands;
using VehicleSales.Attributes;
using VehicleSales.Dtos;
using VehicleSales.Entities.VehicleSale;

namespace VehicleSales.Commands;

public interface IUpdateVehicleSale
{
    Task<Result<ObjectUploadTrackingDto?>> Execute(
        int vehicleSaleId,
        int sellerId,
        UpdateVehicleSaleRequestDto dto,
        CancellationToken cancellationToken);
}

internal sealed class UpdateVehicleSale(
    VehicleSalesDbContext dbContext,
    ICreateObjectUpload createObjectUpload,
    IAmazonS3 r2Client,
    IConfiguration configuration) : IUpdateVehicleSale
{
    // TODO: test cases
    // - just rearrange the order of photos without addition or removal
    // - add new photos and remove some of the existing ones at the same time
    // - remove all photos
    public async Task<Result<ObjectUploadTrackingDto?>> Execute(
        int vehicleSaleId,
        int sellerId,
        UpdateVehicleSaleRequestDto dto,
        CancellationToken cancellationToken)
    {
        var vehicleSale = await dbContext.VehicleSales
            .FirstOrDefaultAsync(
                vehicleSale => vehicleSale.Id == vehicleSaleId && vehicleSale.SellerId == sellerId,
                cancellationToken);
        if (vehicleSale is null)
            return Result.Failure<ObjectUploadTrackingDto?>(
                "Vehicle sale doesn't exist or doesn't belong to the seller");

        vehicleSale.UpdateVehicleSale(
            sale =>
            {
                if (dto.Title is not null)
                    sale.Title = SaleTitle.Create(dto.Title).Value;
                if (dto.Description is not null)
                    sale.Description = SaleDescription.Create(dto.Description).Value;

                var currency = dto.Currency ?? sale.SalePrice.Currency;
                if (dto.AmountInCents is not null)
                    sale.SalePrice = Money.Create(dto.AmountInCents.Value, currency).Value;

                if (dto.Currency is not null)
                    sale.SalePrice = Money.Create(vehicleSale.Sale.SalePrice.AmountInCents, dto.Currency.Value).Value;

                if (dto.County is not null)
                    sale.Location.County = LocationName.Create(dto.County).Value;

                if (dto.Locality is not null)
                    sale.Location.Locality = LocationName.Create(dto.Locality).Value;
            },
            vehicleDetails =>
            {
                if (dto.VehicleModelId is not null)
                    vehicleDetails.VehicleModelId = dto.VehicleModelId.Value;
                if (dto.MileageInKilometers is not null)
                    vehicleDetails.MileageInKilometers = dto.MileageInKilometers;
                if (dto.HorsePower is not null)
                    vehicleDetails.MileageInKilometers = dto.HorsePower;
                if (dto.VehicleVersion is not null)
                    vehicleDetails.VehicleVersion = VehicleVersion.Create(dto.VehicleVersion).Value;
                if (dto.BodyType is not null)
                    vehicleDetails.BodyType = dto.BodyType;
                if (dto.EngineVolumeInCm3 is not null)
                    vehicleDetails.EngineVolumeInCm3 = dto.EngineVolumeInCm3;
                if (dto.ExteriorColor is not null)
                    vehicleDetails.ExteriorColor = ColorName.Create(dto.ExteriorColor).Value;
                if (dto.InteriorColor is not null)
                    vehicleDetails.InteriorColor = ColorName.Create(dto.InteriorColor).Value;
                if (dto.FuelType is not null)
                    vehicleDetails.FuelType = dto.FuelType;
                if (dto.VehicleManufacturingYear is not null)
                    vehicleDetails.VehicleManufacturingYear = VehicleManufacturingYear.Create(
                        dto.VehicleManufacturingYear, DateTime.Now.Year).Value;
                if (dto.VehicleNumberOfDoors is not null)
                    vehicleDetails.VehicleNumberOfDoors = NumberBetween1And9.Create(dto.VehicleNumberOfDoors).Value;
                if (dto.VehicleCondition is not null)
                    vehicleDetails.VehicleCondition = dto.VehicleCondition;
                if (dto.GearboxType is not null)
                    vehicleDetails.GearboxType = dto.GearboxType;
                if (dto.SteeringWheelSide is not null)
                    vehicleDetails.SteeringWheelSide = dto.SteeringWheelSide;
                if (dto.DriveType is not null)
                    vehicleDetails.DriveType = dto.DriveType;
                if (dto.NumberOfSeats is not null)
                    vehicleDetails.NumberOfSeats = dto.NumberOfSeats;
                if (dto.EmissionStandard is not null)
                    vehicleDetails.EmissionStandard = dto.EmissionStandard;
                if (dto.HasServiceHistory is not null)
                    vehicleDetails.HasServiceHistory = dto.HasServiceHistory.Value;
                if (dto.HasAccidentHistory is not null)
                    vehicleDetails.HasAccidentHistory = dto.HasAccidentHistory.Value;
                if (dto.Vin is not null)
                    vehicleDetails.Vin = VIN.Create(dto.Vin).Value;
                if (dto.NumberOfPreviousOwners is not null)
                    vehicleDetails.NumberOfPreviousOwners = dto.NumberOfPreviousOwners;
                if (dto.BatteryCapacityInKWh is not null)
                    vehicleDetails.BatteryCapacityInKWh = dto.BatteryCapacityInKWh;
                if (dto.FuelType is not null)
                    vehicleDetails.FuelType = dto.FuelType;
                if (dto.RangeInKilometers is not null)
                    vehicleDetails.RangeInKilometers = dto.RangeInKilometers;
                if (dto.AverageFuelConsumptionInLitersPer100Km is not null)
                    vehicleDetails.AverageFuelConsumptionInLitersPer100Km = dto.AverageFuelConsumptionInLitersPer100Km;
                if (dto.AverageBatteryConsumptionInKWhPer100Km is not null)
                    vehicleDetails.AverageBatteryConsumptionInKWhPer100Km = dto.AverageBatteryConsumptionInKWhPer100Km;
                if (dto.MassInKg is not null)
                    vehicleDetails.MassInKg = dto.MassInKg;
                if (dto.MaximumLoadInKg is not null)
                    vehicleDetails.MaximumLoadInKg = dto.MaximumLoadInKg;
            });

        if (dto.Photos?.Any() != true)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }

        var contentTypesAndExistingPhotoKeys = dto.Photos.ToLookup(
            MaxPhotoContentTypesAttribute.AllowedImageContentTypes.Contains);

        var existentOrInexistentPhotoKeys = contentTypesAndExistingPhotoKeys[false]
            .ToLookup(existingObj =>
                vehicleSale.VehicleDetails.PhotoKeys?.Any(pk => pk.Value == existingObj) != true);
        var inexistentObjects = existentOrInexistentPhotoKeys[true].ToArray();
        if (inexistentObjects.Length > 0)
            return Result.Failure<ObjectUploadTrackingDto?>(
                $"The following items have an unsupported content type or don't correspond to existing photo keys: {string.Join(", ", inexistentObjects)}");

        var existentPhotoKeys = existentOrInexistentPhotoKeys[false].ToArray();
        var photoKeysToRemove = vehicleSale.VehicleDetails.PhotoKeys?
            .Where(pk => !existentPhotoKeys.Contains(pk.Value))
            .ToArray();
        if (photoKeysToRemove?.Length > 0)
            vehicleSale.VehicleDetails.PhotoKeys!.RemoveAll(pk => photoKeysToRemove.Contains(pk));

        var contentTypes = contentTypesAndExistingPhotoKeys[true];
        if (contentTypes?.Any() != true)
        {
            vehicleSale.UpdateVehicleDetails(
                vehicleDetails =>
                {
                    vehicleDetails.PhotoKeys = [.. dto.Photos.Select(p => ObjectKeyName.Create(p).Value)];
                });
            await dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
        var nextIndex = vehicleSale.VehicleDetails.PhotoKeys?
            .Select(pk => int.Parse(pk.Value.Split('.')[0]))
            .Max() + 1 ?? 0;

        IReadOnlyList<(ObjectKeyName ObjectKey, string ContentType, bool IsNew)> photoKeys =
            CreateObjectKeys(dto.Photos, nextIndex);
        ObjectUpload objectUpload = new()
        {
            EntityId = vehicleSale.Id,
            Directory = vehicleSale.VehicleDetails.Directory ??
                    DirectoryName.Create(Guid.CreateVersion7().ToString("N")).Value,
            ObjectKeys = [.. photoKeys.Select(pk => pk.ObjectKey)],
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
        };
        await createObjectUpload.Execute(
            objectUpload,
            cancellationToken);
        return new ObjectUploadTrackingDto(
            vehicleSale.Id)
        {
            ObjectUploadId = objectUpload.Id,
            ObjectKeysAndTheirPresignedUploadUrls = CreatePresignedPutRequests(
                objectUpload.Directory,
                photoKeys
                    .Where(pk => pk.IsNew)
                    .Select(pk => (pk.ObjectKey, pk.ContentType))
                    .ToList())
        };
    }

    private static IReadOnlyList<(ObjectKeyName ObjectKey, string ContentType, bool IsNew)> CreateObjectKeys(
        IReadOnlyList<string> photoKeysAndContentTypes, int index)
    =>
        [.. photoKeysAndContentTypes
            .Select(photoKeyOrContentType => MaxPhotoContentTypesAttribute.AllowedImageContentTypes.Contains(photoKeyOrContentType)
                ? ($"{index++}.{photoKeyOrContentType.Split('/')[1]}", photoKeyOrContentType, true)
                : (photoKeyOrContentType, string.Empty, false))
            .Select(tuple => (ObjectKeyName.Create(tuple.Item1).Value, tuple.Item2, tuple.Item3))];

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
