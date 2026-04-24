using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VehicleSales.Attributes;
using VehicleSales.Dtos;
using VehicleSales.Entities.VehicleSale;

namespace VehicleSales.Commands;

public enum UpdateVehicleSaleErrorCode
{
    VehicleSaleNotFound,
    InexistentPhotoKeys,
}

public interface IUpdateVehicleSale
{
    Task<UnitResult<UpdateVehicleSaleErrorCode>> Execute(
        int vehicleSaleId,
        int sellerId,
        UpdateVehicleSaleRequestDto dto,
        IFormFileCollection? newPhotos,
        CancellationToken cancellationToken);
}

internal sealed class UpdateVehicleSale(
    VehicleSalesDbContext dbContext,
    IAmazonS3 r2Client,
    IConfiguration configuration) : IUpdateVehicleSale
{
    public async Task<UnitResult<UpdateVehicleSaleErrorCode>> Execute(
        int vehicleSaleId,
        int sellerId,
        UpdateVehicleSaleRequestDto dto,
        IFormFileCollection? newPhotos,
        CancellationToken cancellationToken)
    {
        var vehicleSale = await dbContext.VehicleSales
            .FirstOrDefaultAsync(
                vs => vs.Id == vehicleSaleId && vs.SellerId == sellerId,
                cancellationToken);
        if (vehicleSale is null)
            return UpdateVehicleSaleErrorCode.VehicleSaleNotFound;

        // Validate that all keys in ExistingPhotos actually exist in the current sale.
        var currentKeys = vehicleSale.VehicleDetails.PhotoKeys;
        var currentKeySet = currentKeys?.Select(k => k.Value).ToHashSet() ?? [];
        var inexistent = dto.ExistingPhotos?
            .Where(k => !currentKeySet.Contains(k))
            .ToList() ?? [];
        if (inexistent.Count > 0)
            return UpdateVehicleSaleErrorCode.InexistentPhotoKeys;

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
                    vehicleDetails.HorsePower = dto.HorsePower;
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

                // Apply photo key changes: keep only requested existing keys in stated order.
                if (dto.ExistingPhotos is not null)
                    vehicleDetails.PhotoKeys = [.. dto.ExistingPhotos
                        .Select(k => ObjectKeyName.Create(k).Value)];
            });

        // Delete photos that were removed.
        if (dto.ExistingPhotos is not null && vehicleSale.VehicleDetails.Directory is not null)
        {
            var removedKeys = currentKeySet.Except(dto.ExistingPhotos).ToList();
            if (removedKeys.Count > 0)
            {
                var bucketName = configuration[R2Config.BucketNameKey] ??
                    throw new InvalidOperationException("Bucket name not set in configuration");
                await r2Client.DeleteObjectsAsync(
                    new DeleteObjectsRequest
                    {
                        BucketName = bucketName,
                        Objects = [.. removedKeys.Select(k => new KeyVersion
                        {
                            Key = $"{vehicleSale.VehicleDetails.Directory.Value}/{k}"
                        })]
                    },
                    cancellationToken);
            }
        }

        // Upload new photos.
        if (newPhotos is { Count: > 0 })
        {
            var bucketName = configuration[R2Config.BucketNameKey] ??
                throw new InvalidOperationException("Bucket name not set in configuration");

            // Reuse the existing directory if one exists, otherwise create a new one.
            var directory = vehicleSale.VehicleDetails.Directory
                ?? DirectoryName.Create(Guid.CreateVersion7().ToString("N")).Value;

            // Next index is based on the highest existing index + 1.
            var nextIndex = vehicleSale.VehicleDetails.PhotoKeys?
                .Select(pk => int.TryParse(pk.Value.Split('.')[0], out var idx) ? idx : -1)
                .DefaultIfEmpty(-1)
                .Max() + 1 ?? 0;

            var appendedKeys = new List<ObjectKeyName>(newPhotos.Count);
            for (int i = 0; i < newPhotos.Count; i++)
            {
                var photo = newPhotos[i];
                var extension = photo.ContentType.Split('/')[1];
                var key = ObjectKeyName.Create($"{nextIndex + i}.{extension}").Value;

                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = $"{directory.Value}/{key.Value}",
                    InputStream = photo.OpenReadStream(),
                    ContentType = photo.ContentType,
                    DisablePayloadSigning = true
                };

                await r2Client.PutObjectAsync(putRequest, cancellationToken);
                appendedKeys.Add(key);
            }

            vehicleSale.UpdateVehicleDetails(vd =>
            {
                vd.Directory = directory;
                vd.PhotoKeys = [.. (vd.PhotoKeys ?? []), .. appendedKeys];
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<UpdateVehicleSaleErrorCode>();
    }
}
