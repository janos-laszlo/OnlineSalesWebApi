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

        var diff = new ObjectDiff(
            vehicleSale.VehicleDetails.PhotoKeys,
            dto.Photos);

        if (diff.Inexistent.Count > 0)
            return Result.Failure<ObjectUploadTrackingDto?>(
                $"{string.Join(", ", diff.Inexistent)} photos do not exist.");

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
                if (dto.Photos is not null)
                {
                    // Cases(subsets of reordering, addition and removal):
                    // - do nothing with photos -> no action needed
                    // - just reorder photos -> assign new version here
                    // - just remove some photos -> assign new version here and remove the photos from R2
                    // - just add some photos -> no reassignment here, assign new version in ConfirmObjectUploadForVehicleSale when confirming the upload of the new photos
                    // - reorder photos and add new ones without removal -> create ObjectUpload for the new photos and assign the new version including the new photos in ConfirmObjectUploadForVehicleSale
                    // - reorder photos and remove some without addition -> same as 'just remove some photos' case, the new version will be assigned here and the removed photos will be deleted from R2
                    // - add and remove photos without reordering -> reassign NewVersion except Added here, create ObjectUpload for Added and remove from R2 the Removed photos.
                    // - reorder photos, add new ones and remove some at the same time -> assign NewVersion except Added here, create ObjectUpload for the new photos and assign the new version including the new photos in ConfirmObjectUploadForVehicleSale, remove from R2 the Removed photos.

                    vehicleDetails.PhotoKeys = diff.NewVersion?
                        .Except(diff.Added)
                        .ToList();
                }
            });

        if (diff.Removed.Count > 0)
            await r2Client.DeleteObjectsAsync(
                new DeleteObjectsRequest
                {
                    BucketName = configuration[R2Config.BucketNameKey],
                    Objects = [.. diff.Removed
                        .Select(objectKey => new KeyVersion
                        {
                            Key = $"{vehicleSale.VehicleDetails.Directory?.Value}/{objectKey}"
                        })]
                },
                cancellationToken);

        ObjectUploadTrackingDto? result = null;
        if (diff.Added.Count > 0)
        {
            ObjectUpload objectUpload = new()
            {
                Module = Constants.ModuleName,
                EntityId = vehicleSale.Id,
                Directory = vehicleSale.VehicleDetails.Directory ??
                        DirectoryName.Create(Guid.CreateVersion7().ToString("N")).Value,
                ObjectKeys = diff.Added,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            };

            await createObjectUpload.Execute(
                objectUpload,
                cancellationToken);

            result = new ObjectUploadTrackingDto(
                vehicleSale.Id)
            {
                ObjectUploadId = objectUpload.Id,
                ObjectKeysAndTheirPresignedUploadUrls = CreatePresignedPutRequests(
                    objectUpload.Directory,
                    diff.AddedObjectKeysAndContentTypes)
            };
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return result ?? new ObjectUploadTrackingDto(
            vehicleSale.Id);
    }

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

    private sealed class ObjectDiff
    {
        public IReadOnlyList<ObjectKeyName>? NewVersion { get; }
        public IReadOnlyList<string> Removed { get; }
        public IReadOnlyList<ObjectKeyName> Added { get; }
        public IReadOnlyList<(ObjectKeyName, string)> AddedObjectKeysAndContentTypes { get; }
        public IReadOnlyList<string> Inexistent { get; }

        internal ObjectDiff(
            IEnumerable<ObjectKeyName>? currentPhotoKeys,
            IEnumerable<string>? newPhotoKeys)
        {
            var nextIndex =
                currentPhotoKeys?
                    .Select(pk => int.Parse(pk.Value.Split('.')[0]))
                    .Max() + 1 ?? 0;

            NewVersion = GetNewVersion(currentPhotoKeys, newPhotoKeys, nextIndex);
            Removed = GetRemoved(currentPhotoKeys, newPhotoKeys);
            Added = GetAdded(newPhotoKeys, nextIndex);
            AddedObjectKeysAndContentTypes = GetAddedObjectKeyAndContentType(newPhotoKeys, nextIndex);
            Inexistent = GetInexistent(newPhotoKeys, currentPhotoKeys);
        }

        static List<ObjectKeyName>? GetNewVersion(IEnumerable<ObjectKeyName>? currentPhotoKeys, IEnumerable<string>? newPhotoKeys, int nextIndex)
        {
            var currentIndex = nextIndex;
            return newPhotoKeys is null
                ? currentPhotoKeys?.ToList()
                : newPhotoKeys
                    .Intersect(MaxPhotoContentTypesAttribute.AllowedImageContentTypes)
                    .Select(npk => MaxPhotoContentTypesAttribute.AllowedImageContentTypes.Contains(npk)
                        ? ObjectKeyName.Create($"{currentIndex++}.{npk.Split('/')[1]}").Value
                        : ObjectKeyName.Create(npk).Value)
                    .ToList();
        }

        private static List<string> GetRemoved(
            IEnumerable<ObjectKeyName>? currentPhotoKeys,
            IEnumerable<string>? newPhotoKeys)
        =>
            newPhotoKeys is null
                ? []
                : currentPhotoKeys?
                    .Select(pk => pk.Value)
                    .Except(newPhotoKeys)
                    .ToList()
                    ?? [];

        private static List<ObjectKeyName> GetAdded(
            IEnumerable<string>? newPhotoKeys,
            int nextIndex)
        {
            var currentIndex = nextIndex;
            return newPhotoKeys?
                .Intersect(MaxPhotoContentTypesAttribute.AllowedImageContentTypes)
                .Select(npk => ObjectKeyName.Create($"{currentIndex++}.{npk.Split('/')[1]}").Value)
                .ToList()
            ?? [];
        }

        private static List<(ObjectKeyName, string)> GetAddedObjectKeyAndContentType(
            IEnumerable<string>? newPhotoKeys,
            int nextIndex)
        {
            var currentIndex = nextIndex;
            return newPhotoKeys?
                .Intersect(MaxPhotoContentTypesAttribute.AllowedImageContentTypes)
                .Select(npk => (ObjectKeyName.Create($"{currentIndex++}.{npk.Split('/')[1]}").Value, npk))
                .ToList()
            ?? [];
        }

        private static List<string> GetInexistent(
            IEnumerable<string>? newPhotoKeys,
            IEnumerable<ObjectKeyName>? currentPhotoKeys)
        =>
            newPhotoKeys?
                .Except(MaxPhotoContentTypesAttribute.AllowedImageContentTypes
                    .Concat(currentPhotoKeys?.Select(pk => pk.Value) ?? []))
                .ToList()
            ?? [];
    }
}
