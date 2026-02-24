using CSharpFunctionalExtensions;
using VehicleSales.Entities.VehicleSale;

namespace VehicleSales.Commands;

public interface ICreateVehicleSale
{
    Task<Result> Execute(CreateVehicleSaleDto dto);
}

internal sealed class CreateVehicleSale : ICreateVehicleSale
{
    public async Task<Result> Execute(CreateVehicleSaleDto dto)
    {
        var vehicleSaleResult = dto.ToVehicleSale();
        if (vehicleSaleResult.IsFailure)
            return Result.Failure(vehicleSaleResult.Error);

        await Task.Delay(100);
        return Result.Success();
    }
}

public sealed record CreateVehicleSaleDto(
    string? Title,
    string? Description,
    int? AmountInCents,
    string? Currency,
    string? County,
    string? Locality,
    int VehicleModelId,
    uint MileageInKilometers,
    uint? HorsePower,
    string? VehicleVersion,
    string? BodyType,
    uint? EngineVolumeInCm3,
    string? ExteriorColor,
    string? InteriorColor,
    string? FuelType,
    int? VehicleManufacturingYear,
    int? VehicleNumberOfDoors,
    string? VehicleCondition,
    string? GearboxType,
    string? SteeringWheelSide,
    string? DriveType,
    int? NumberOfSeats,
    string? EmissionStandard,
    bool? HasServiceHistory,
    bool? HasAccidentHistory,
    string? Vin,
    ushort? NumberOfPreviousOwners,
    uint? BatteryCapacityInKWh,
    uint? RangeInKilometers,
    uint? AverageFuelConsumptionInLitersPer100Km,
    ushort? AverageBatteryConsumptionInKWhPer100Km,
    uint? Mass,
    uint? MaximumLoad)
{
    internal Result<VehicleSale> ToVehicleSale()
    {
        var saleTitleResult = SaleTitle.Create(Title);
        if (saleTitleResult.IsFailure)
            return Result.Failure<VehicleSale>(saleTitleResult.Error);

        var saleDescriptionResult = SaleDescription.Create(Description);
        if (saleDescriptionResult.IsFailure)
            return Result.Failure<VehicleSale>(saleDescriptionResult.Error);

        var currencyResult = GetEnumParsingResult<Currency>(
            Currency, "Invalid currency");
        if (currencyResult.IsFailure)
            return Result.Failure<VehicleSale>(currencyResult.Error);

        var salePriceResult = Money.Create(AmountInCents, currencyResult.Value);
        if (salePriceResult.IsFailure)
            return Result.Failure<VehicleSale>(salePriceResult.Error);

        var countyResult = MinLength3String.Create(County);
        if (countyResult.IsFailure)
            return Result.Failure<VehicleSale>(countyResult.Error);

        var localityResult = MinLength3String.Create(Locality);
        if (localityResult.IsFailure)
            return Result.Failure<VehicleSale>(localityResult.Error);

        var bodyTypeResult = GetEnumParsingResult<BodyType>(
            BodyType, "Invalid body type");
        if (bodyTypeResult.IsFailure)
            return Result.Failure<VehicleSale>(bodyTypeResult.Error);

        var exteriorColorResult = MinLength3String.Create(ExteriorColor);
        if (exteriorColorResult.IsFailure)
            return Result.Failure<VehicleSale>(exteriorColorResult.Error);

        var interiorColorResult = MinLength3String.Create(InteriorColor);
        if (interiorColorResult.IsFailure)
            return Result.Failure<VehicleSale>(interiorColorResult.Error);

        var fuelTypeResult = GetEnumParsingResult<FuelType>(
            FuelType, "Invalid fuel type");
        if (fuelTypeResult.IsFailure)
            return Result.Failure<VehicleSale>(fuelTypeResult.Error);

        var vehicleManufacturingYearResult = Entities.VehicleSale.VehicleManufacturingYear.Create(
            VehicleManufacturingYear, DateTime.Now.Year);
        if (vehicleManufacturingYearResult.IsFailure)
            return Result.Failure<VehicleSale>(vehicleManufacturingYearResult.Error);

        var numberOfDoorsResult = NumberBetween1And9.Create(VehicleNumberOfDoors);
        if (numberOfDoorsResult.IsFailure)
            return Result.Failure<VehicleSale>("Invalid number of doors");

        var vehicleConditionResult = GetEnumParsingResult<VehicleCondition>(
            VehicleCondition, "Invalid vehicle condition");
        if (vehicleConditionResult.IsFailure)
            return Result.Failure<VehicleSale>(vehicleConditionResult.Error);

        var gearboxTypeResult = GetEnumParsingResult<GearboxType>(
            GearboxType, "Invalid gearbox type");
        if (gearboxTypeResult.IsFailure)
            return Result.Failure<VehicleSale>(gearboxTypeResult.Error);

        var sideResult = GetEnumParsingResult<Side>(
            SteeringWheelSide, "Invalid steering wheel side");
        if (sideResult.IsFailure)
            return Result.Failure<VehicleSale>(sideResult.Error);

        var driveTypeResult = GetEnumParsingResult<Entities.VehicleSale.DriveType>(
            DriveType, "Invalid drive type");
        if (driveTypeResult.IsFailure)
            return Result.Failure<VehicleSale>(driveTypeResult.Error);

        var numberOfSeatsResult = NumberBetween1And9.Create(NumberOfSeats);
        if (numberOfSeatsResult.IsFailure)
            return Result.Failure<VehicleSale>("Invalid number of seats");

        var emissionStandardResult = GetEnumParsingResult<EmissionStandard>(
            EmissionStandard, "Invalid emission standard");
        if (emissionStandardResult.IsFailure)
            return Result.Failure<VehicleSale>(emissionStandardResult.Error);

        var vinResult = VIN.Create(Vin);
        if (vinResult.IsFailure)
            return Result.Failure<VehicleSale>(vinResult.Error);

        return new VehicleSale
        {
            SellerId = 0, // TODO: Get from context
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
                VehicleVersion = VehicleVersion,
                BodyType = bodyTypeResult.Value,
                EngineVolumeInCm3 = EngineVolumeInCm3,
                ExteriorColor = exteriorColorResult.Value,
                InteriorColor = interiorColorResult.Value,
                FuelType = fuelTypeResult.Value,
                VehicleManufacturingYear = vehicleManufacturingYearResult.Value,
                VehicleNumberOfDoors = numberOfDoorsResult.Value,
                VehicleCondition = vehicleConditionResult.Value,
                GearboxType = gearboxTypeResult.Value,
                SteeringWheelSide = sideResult.Value,
                DriveType = driveTypeResult.Value,
                NumberOfSeats = numberOfSeatsResult.Value,
                EmissionStandard = emissionStandardResult.Value,
                HasServiceHistory = HasServiceHistory,
                HasAccidentHistory = HasAccidentHistory,
                Vin = vinResult.Value,
                NumberOfPreviousOwners = NumberOfPreviousOwners,
                BatteryCapacityInKWh = BatteryCapacityInKWh,
                RangeInKilometers = RangeInKilometers,
                AverageFuelConsumptionInLitersPer100Km = AverageFuelConsumptionInLitersPer100Km,
                AverageBatteryConsumptionInKWhPer100Km = AverageBatteryConsumptionInKWhPer100Km,
                Mass = Mass,
                MaximumLoad = MaximumLoad
            }
        };
    }

    private static Result<T> GetEnumParsingResult<T>(
        string? value,
        string errorMessage) where T : struct, Enum
    =>
        Enum.TryParse<T>(value, true, out T bodyType)
            ? bodyType
            : Result.Failure<T>(errorMessage);
}
