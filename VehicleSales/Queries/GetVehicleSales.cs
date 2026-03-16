using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using ObjectUploadTracking;
using VehicleSales.Entities.VehicleSale;

namespace VehicleSales.Queries;

public interface IGetVehicleSales
{
    Task<IReadOnlyList<VehicleSaleDto>> Execute(PagedRequest request, CancellationToken cancellation);
}

internal sealed class GetVehicleSales(
    VehicleSalesDbContext dbContext) : IGetVehicleSales
{
    public async Task<IReadOnlyList<VehicleSaleDto>> Execute(PagedRequest request, CancellationToken cancellation) =>
        // TODO: Use SqlRaw or Dapper because EF parses each value object and this results in a very inefficient query.
        await dbContext.VehicleSales
            .Skip((request.PageNumber) * request.PageSize)
            .Take(request.PageSize)
            .Select(vs =>
                new VehicleSaleDto(
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    vs.Id,
                    vs.Sale.Title.Value,
                    vs.Sale.Description.Value,
                    vs.Sale.SalePrice.AmountInCents,
                    vs.Sale.SalePrice.Currency,
                    vs.Sale.Location.County.Value,
                    vs.Sale.Location.Locality.Value,
                    vs.VehicleDetails.VehicleModelId)
                {
                    MileageInKilometers = vs.VehicleDetails.MileageInKilometers,
                    HorsePower = vs.VehicleDetails.HorsePower,
                    VehicleVersion = vs.VehicleDetails.VehicleVersion.Value,
                    BodyType = vs.VehicleDetails.BodyType,
                    EngineVolumeInCm3 = vs.VehicleDetails.EngineVolumeInCm3,
                    ExteriorColor = vs.VehicleDetails.ExteriorColor.Value,
                    InteriorColor = vs.VehicleDetails.InteriorColor.Value,
                    FuelType = vs.VehicleDetails.FuelType,
                    VehicleManufacturingYear = vs.VehicleDetails.VehicleManufacturingYear.Value,
                    VehicleNumberOfDoors = vs.VehicleDetails.VehicleNumberOfDoors.Value,
                    VehicleCondition = vs.VehicleDetails.VehicleCondition,
                    GearboxType = vs.VehicleDetails.GearboxType,
                    SteeringWheelSide = vs.VehicleDetails.SteeringWheelSide,
                    DriveType = vs.VehicleDetails.DriveType,
                    NumberOfSeats = vs.VehicleDetails.NumberOfSeats,
                    EmissionStandard = vs.VehicleDetails.EmissionStandard,
                    HasServiceHistory = vs.VehicleDetails.HasServiceHistory,
                    HasAccidentHistory = vs.VehicleDetails.HasAccidentHistory,
                    Vin = vs.VehicleDetails.Vin.Value,
                    NumberOfPreviousOwners = vs.VehicleDetails.NumberOfPreviousOwners,
                    BatteryCapacityInKWh = vs.VehicleDetails.BatteryCapacityInKWh,
                    RangeInKilometers = vs.VehicleDetails.RangeInKilometers,
                    AverageFuelConsumptionInLitersPer100Km = vs.VehicleDetails.AverageFuelConsumptionInLitersPer100Km,
                    AverageBatteryConsumptionInKWhPer100Km = vs.VehicleDetails.AverageBatteryConsumptionInKWhPer100Km,
                    MassInKg = vs.VehicleDetails.MassInKg,
                    MaximumLoadInKg = vs.VehicleDetails.MaximumLoadInKg,
                    Directory = vs.VehicleDetails.Directory.Value,
                    PhotoKeysInternal = vs.VehicleDetails.PhotoKeys
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                })
            .ToListAsync(cancellation);
}

public sealed record VehicleSaleDto(
    int Id,

    [property: Description("The display title of the sale listing.")]
    [property: MinLength(SaleTitle.MinLength), MaxLength(SaleTitle.MaxLength)]
    string Title,

    [property: Description("A detailed description of the vehicle.")]
    [property: MinLength(SaleDescription.MinLength), MaxLength(SaleDescription.MaxLength)]
    string Description,

    [property: Description("Sale price in cents (e.g. 150000 = $1,500).")]
    uint AmountInCents,

    [property: Description("Currency of the sale price.")]
    Currency Currency,

    [property: Description("County/region where the vehicle is located.")]
    [property: MinLength(LocationName.MinLength), MaxLength(LocationName.MaxLength)]
    string County,

    [property: Description("City/locality where the vehicle is located.")]
    [property: MinLength(LocationName.MinLength), MaxLength(LocationName.MaxLength)]
    string Locality,

    [property: Description("ID of the vehicle model.")]
    int VehicleModelId)
{
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

    public string? Directory { get; init; }
    internal IReadOnlyList<ObjectKeyName>? PhotoKeysInternal { get; init; }
    public IReadOnlyList<string>? PhotoKeys { get; set; }
}
