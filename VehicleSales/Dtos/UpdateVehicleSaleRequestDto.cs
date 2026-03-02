using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VehicleSales.Attributes;
using VehicleSales.Entities.VehicleSale;

namespace VehicleSales.Dtos;

public sealed record UpdateVehicleSaleRequestDto
{
    [Description("The display title of the sale listing.")]
    [MinLength(SaleTitle.MinLength), MaxLength(SaleTitle.MaxLength)]
    public string? Title { get; init; }

    [Description("A detailed description of the vehicle.")]
    [MinLength(SaleDescription.MinLength), MaxLength(SaleDescription.MaxLength)]
    public string? Description { get; init; }

    [Description("Sale price in cents (e.g. 150000 = $1,500).")]
    public uint? AmountInCents { get; init; }

    [Description("Currency of the sale price.")]
    public Currency? Currency { get; init; }

    [Description("County/region where the vehicle is located.")]
    [MinLength(LocationName.MinLength), MaxLength(LocationName.MaxLength)]
    public string? County { get; init; }

    [Description("City/locality where the vehicle is located.")]
    [MinLength(LocationName.MinLength), MaxLength(LocationName.MaxLength)]
    public string? Locality { get; init; }

    [Description("ID of the vehicle model.")]
    [VehicleModelId]
    public int? VehicleModelId { get; init; }

    [Description("Total mileage on the vehicle in kilometers.")]
    public uint? MileageInKilometers { get; init; }

    public uint? HorsePower { get; init; }

    [Description("Additional info to the vehicle model")]
    [MinLength(Entities.VehicleSale.VehicleVersion.MinLength)]
    [MaxLength(Entities.VehicleSale.VehicleVersion.MaxLength)]
    public string? VehicleVersion { get; init; }

    public BodyType? BodyType { get; init; }

    public uint? EngineVolumeInCm3 { get; init; }

    [MinLength(ColorName.MinLength), MaxLength(ColorName.MaxLength)]
    public string? ExteriorColor { get; init; }

    [MinLength(ColorName.MinLength), MaxLength(ColorName.MaxLength)]
    public string? InteriorColor { get; init; }

    public FuelType? FuelType { get; init; }

    [Description($"Year the vehicle was manufactured. Must be >= 1880 and <= current year")]
    public ushort? VehicleManufacturingYear { get; init; }

    [Range(NumberBetween1And9.One, NumberBetween1And9.Nine)]
    public ushort? VehicleNumberOfDoors { get; init; }

    public VehicleCondition? VehicleCondition { get; init; }

    public GearboxType? GearboxType { get; init; }

    public Side? SteeringWheelSide { get; init; }

    public Entities.VehicleSale.DriveType? DriveType { get; init; }

    public ushort? NumberOfSeats { get; init; }

    public EmissionStandard? EmissionStandard { get; init; }

    public bool? HasServiceHistory { get; init; }

    public bool? HasAccidentHistory { get; init; }

    [Description("Vehicle Identification Number.")]
    [MinLength(VIN.MinLength), MaxLength(VIN.MaxLength)]
    public string? Vin { get; init; }

    public ushort? NumberOfPreviousOwners { get; init; }

    [Description("Battery capacity in kilowatt-hours. For electric/hybrid vehicles.")]
    public uint? BatteryCapacityInKWh { get; init; }

    public uint? RangeInKilometers { get; init; }

    [Description("Average fuel consumption in liters per 100 km.")]
    public uint? AverageFuelConsumptionInLitersPer100Km { get; init; }

    [Description("Average battery consumption in kWh per 100 km.")]
    public ushort? AverageBatteryConsumptionInKWhPer100Km { get; init; }

    [Description("Unladen mass of the vehicle in kilograms.")]
    public uint? MassInKg { get; init; }

    [Description("Maximum load capacity in kilograms.")]
    public uint? MaximumLoadInKg { get; init; }

    [Description(
        """
        Existing photos of the vehicle and new ones specified as content type.
        If a photo is not included in the list, it will be deleted.
        If a content type is included, but there is no existing photo with that content type,
        a presigned URL for that content type will be generated,
        and the client can use it to upload a new photo.
        Content types of presigned URLs. The response will contain presigned URLs
        for the provided content types, which can be used to upload photos of the vehicle.
        Allowed content types: image/jpeg, image/png, image/webp, image/bmp, image/tiff, image/avif.
        Example: ["image/jpeg", "image/png", "0.png", "image/jpeg"]
        """)]
    [MaxLength(VehicleDetails.MaxNumberOfPhotos)]
    public IReadOnlyList<string>? Photos { get; init; }
}
