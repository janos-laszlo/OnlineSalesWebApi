using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using VehicleSales.Entities.VehicleSale;

namespace VehicleSales.Commands;

public interface ICreateVehicleSale
{
    Task<Result> Execute(
        CreateVehicleSaleDto dto,
        int sellerId,
        CancellationToken cancellationToken);
}

internal sealed class CreateVehicleSale(
    VehicleSalesDbContext dbContext) : ICreateVehicleSale
{
    public async Task<Result> Execute(
        CreateVehicleSaleDto dto,
        int sellerId,
        CancellationToken cancellationToken)
    {
        var vehicleSaleResult = dto.ToVehicleSale(sellerId);
        if (vehicleSaleResult.IsFailure)
            return Result.Failure(vehicleSaleResult.Error);

        if (!await dbContext.UsersReadOnly.AnyAsync(
            user => user.Id == sellerId,
            cancellationToken))
        {
            return Result.Failure("Seller not found");
        }

        if (!await dbContext.VehicleModels.AnyAsync(
            vehicleModel => vehicleModel.Id == vehicleSaleResult.Value.VehicleDetails.VehicleModelId,
            cancellationToken))
        {
            return Result.Failure("Vehicle model not found");
        }

        dbContext.VehicleSales.Add(vehicleSaleResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record CreateVehicleSaleDto(
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

    internal Result<VehicleSale> ToVehicleSale(int sellerId)
    {
        // Required fields validation.
        var saleTitleResult = SaleTitle.Create(Title);
        if (saleTitleResult.IsFailure)
            return Result.Failure<VehicleSale>(saleTitleResult.Error);

        var saleDescriptionResult = SaleDescription.Create(Description);
        if (saleDescriptionResult.IsFailure)
            return Result.Failure<VehicleSale>(saleDescriptionResult.Error);

        var salePriceResult = Money.Create(AmountInCents, Currency);
        if (salePriceResult.IsFailure)
            return Result.Failure<VehicleSale>(salePriceResult.Error);

        var countyResult = LocationName.Create(County);
        if (countyResult.IsFailure)
            return Result.Failure<VehicleSale>(countyResult.Error);

        var localityResult = LocationName.Create(Locality);
        if (localityResult.IsFailure)
            return Result.Failure<VehicleSale>(localityResult.Error);

        // Optional fields validation.
        VehicleVersion? vehicleVersion = null;
        if (VehicleVersion is not null)
        {
            var vehicleVersionResult = Entities.VehicleSale.VehicleVersion.Create(VehicleVersion);
            if (vehicleVersionResult.IsFailure)
                return Result.Failure<VehicleSale>(vehicleVersionResult.Error);
            vehicleVersion = vehicleVersionResult.Value;
        }

        ColorName? exteriorColor = null;
        if (ExteriorColor is not null)
        {
            var exteriorColorResult = ColorName.Create(ExteriorColor);
            if (exteriorColorResult.IsFailure)
                return Result.Failure<VehicleSale>(exteriorColorResult.Error);
            exteriorColor = exteriorColorResult.Value;
        }

        ColorName? interiorColor = null;
        if (InteriorColor is not null)
        {
            var interiorColorResult = ColorName.Create(InteriorColor);
            if (interiorColorResult.IsFailure)
                return Result.Failure<VehicleSale>(interiorColorResult.Error);
            exteriorColor = interiorColorResult.Value;
        }

        VehicleManufacturingYear? vehicleManufacturingYear = null;
        if (VehicleManufacturingYear is not null)
        {
            var vehicleManufacturingYearResult = Entities.VehicleSale.VehicleManufacturingYear.Create(
                VehicleManufacturingYear, DateTime.Now.Year);
            if (vehicleManufacturingYearResult.IsFailure)
                return Result.Failure<VehicleSale>(vehicleManufacturingYearResult.Error);
            vehicleManufacturingYear = vehicleManufacturingYearResult.Value;
        }

        NumberBetween1And9? numberOfDoors = null;
        if (numberOfDoors is not null)
        {
            var numberOfDoorsResult = NumberBetween1And9.Create(VehicleNumberOfDoors);
            if (numberOfDoorsResult.IsFailure)
                return Result.Failure<VehicleSale>("Invalid number of doors");
            numberOfDoors = numberOfDoorsResult.Value;
        }

        VIN? vin = null;
        if (Vin is not null)
        {
            var vinResult = VIN.Create(Vin);
            if (vinResult.IsFailure)
                return Result.Failure<VehicleSale>(vinResult.Error);
            vin = vinResult.Value;
        }

        return new VehicleSale
        {
            SellerId = sellerId,
            Sale = new Sale
            {
                Title = saleTitleResult.Value,
                Description = saleDescriptionResult.Value,
                SalePrice = salePriceResult.Value,
                Location = new Location(countyResult.Value, localityResult.Value),
            },
            VehicleDetails = new VehicleDetails
            {
                VehicleModelId = VehicleModelId,
                MileageInKilometers = MileageInKilometers,
                HorsePower = HorsePower,
                VehicleVersion = vehicleVersion,
                BodyType = BodyType,
                EngineVolumeInCm3 = EngineVolumeInCm3,
                ExteriorColor = exteriorColor,
                InteriorColor = interiorColor,
                FuelType = FuelType,
                VehicleManufacturingYear = vehicleManufacturingYear,
                VehicleNumberOfDoors = numberOfDoors,
                VehicleCondition = VehicleCondition,
                GearboxType = GearboxType,
                SteeringWheelSide = SteeringWheelSide,
                DriveType = DriveType,
                NumberOfSeats = NumberOfSeats,
                EmissionStandard = EmissionStandard,
                HasServiceHistory = HasServiceHistory,
                HasAccidentHistory = HasAccidentHistory,
                Vin = vin,
                NumberOfPreviousOwners = NumberOfPreviousOwners,
                BatteryCapacityInKWh = BatteryCapacityInKWh,
                RangeInKilometers = RangeInKilometers,
                AverageFuelConsumptionInLitersPer100Km = AverageFuelConsumptionInLitersPer100Km,
                AverageBatteryConsumptionInKWhPer100Km = AverageBatteryConsumptionInKWhPer100Km,
                MassInKg = MassInKg,
                MaximumLoadInKg = MaximumLoadInKg
            }
        };
    }
}
