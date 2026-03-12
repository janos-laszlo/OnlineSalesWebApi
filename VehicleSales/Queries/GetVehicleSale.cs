using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace VehicleSales.Queries;

public interface IGetVehicleSale
{
    Task<VehicleSaleDto?> Execute(int id, CancellationToken cancellation);
}

internal sealed class GetVehicleSale(
    VehicleSalesDbContext dbContext) : IGetVehicleSale
{
    public async Task<VehicleSaleDto?> Execute(int id, CancellationToken cancellation) =>
        // TODO: Use SqlRaw or Dapper because EF parses each value object and
        // this results in a very inefficient query.
        await dbContext.VehicleSales
            .Where(vs => vs.Id == id)
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
            .FirstOrDefaultAsync(cancellation);
}
