using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using VehicleSales.Entities.VehicleSale;
using static Common.Constants;

namespace VehicleSales.Queries;

public interface IGetVehicleSale
{
    Task<VehicleSaleFullDto?> Execute(int id, CancellationToken cancellation);
}

internal sealed class GetVehicleSale(
    IConfiguration configuration) : IGetVehicleSale
{
    public async Task<VehicleSaleFullDto?> Execute(int id, CancellationToken cancellation)
    {
        var connectionString = configuration.GetConnectionString(ConfigKeys.ConnectionStringKey);
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellation);
        string query = 
            $"""
            SELECT 
                {nameof(VehicleSale.Id)},
                {nameof(VehicleSale.SellerId)},
                {nameof(Sale.Title)},
                {nameof(Sale.Description)},
                {nameof(Money.AmountInCents)},
                {nameof(Money.Currency)},
                {nameof(Location.County)},
                {nameof(Location.Locality)},
                {nameof(VehicleDetails.VehicleModelId)},
                {nameof(VehicleDetails.MileageInKilometers)},
                {nameof(VehicleDetails.HorsePower)},
                {nameof(VehicleDetails.VehicleVersion)},
                {nameof(VehicleDetails.BodyType)},
                {nameof(VehicleDetails.EngineVolumeInCm3)},
                {nameof(VehicleDetails.ExteriorColor)},
                {nameof(VehicleDetails.InteriorColor)},
                {nameof(VehicleDetails.FuelType)},
                {nameof(VehicleDetails.VehicleManufacturingYear)},
                {nameof(VehicleDetails.VehicleNumberOfDoors)},
                {nameof(VehicleDetails.VehicleCondition)},
                {nameof(VehicleDetails.GearboxType)},
                {nameof(VehicleDetails.SteeringWheelSide)},
                {nameof(VehicleDetails.DriveType)},
                {nameof(VehicleDetails.NumberOfSeats)},
                {nameof(VehicleDetails.EmissionStandard)},
                {nameof(VehicleDetails.HasServiceHistory)},
                {nameof(VehicleDetails.HasAccidentHistory)},
                {nameof(VehicleDetails.Vin)},
                {nameof(VehicleDetails.NumberOfPreviousOwners)},
                {nameof(VehicleDetails.BatteryCapacityInKWh)},
                {nameof(VehicleDetails.RangeInKilometers)},
                {nameof(VehicleDetails.AverageFuelConsumptionInLitersPer100Km)},
                {nameof(VehicleDetails.AverageBatteryConsumptionInKWhPer100Km)},
                {nameof(VehicleDetails.MassInKg)},
                {nameof(VehicleDetails.MaximumLoadInKg)},
                {nameof(VehicleDetails.Directory)},
                {nameof(VehicleDetails.PhotoKeys)}
            FROM {Tables.VehicleSales}
            WHERE {nameof(VehicleSale.Id)} = @Id;
            """;
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellation);
        while (await reader.ReadAsync(cancellation))
        {
            return new VehicleSaleFullDto(
                Id: reader.GetInt32(0),
                SellerId: reader.GetInt32(1),
                Title: reader.GetString(2),
                Description: reader.GetString(3),
                AmountInCents: reader.GetUInt32(4),
                Currency: (Currency)reader.GetInt32(5),
                County: reader.GetString(6),
                Locality: reader.GetString(7),
                VehicleModelId: reader.GetInt32(8))
            {
                MileageInKilometers = reader.IsDBNull(9) ? null : reader.GetUInt32(9),
                HorsePower = reader.IsDBNull(10) ? null : reader.GetUInt32(10),
                VehicleVersion = reader.IsDBNull(11) ? null : reader.GetString(11),
                BodyType = reader.IsDBNull(12) ? null : (BodyType)reader.GetUInt32(12),
                EngineVolumeInCm3 = reader.IsDBNull(13) ? null : reader.GetUInt32(13),
                ExteriorColor = reader.IsDBNull(14) ? null : reader.GetString(14),
                InteriorColor = reader.IsDBNull(15) ? null : reader.GetString(15),
                FuelType = reader.IsDBNull(16) ? null : (FuelType)reader.GetUInt32(16),
                VehicleManufacturingYear = reader.IsDBNull(17) ? null : reader.GetUInt16(17),
                VehicleNumberOfDoors = reader.IsDBNull(18) ? null : reader.GetUInt16(18),
                VehicleCondition = reader.IsDBNull(19) ? null : (VehicleCondition)reader.GetUInt32(19),
                GearboxType = reader.IsDBNull(20) ? null : (GearboxType)reader.GetUInt32(20),
                SteeringWheelSide = reader.IsDBNull(21) ? null : (Side)reader.GetUInt32(21),
                DriveType = reader.IsDBNull(22) ? null : (Entities.VehicleSale.DriveType)reader.GetUInt32(22),
                NumberOfSeats = reader.IsDBNull(23) ? null : reader.GetUInt16(23),
                EmissionStandard = reader.IsDBNull(24) ? null : (EmissionStandard)reader.GetUInt32(24),
                HasServiceHistory = reader.IsDBNull(25) ? null : reader.GetBoolean(25),
                HasAccidentHistory = reader.IsDBNull(26) ? null : reader.GetBoolean(26),
                Vin = reader.IsDBNull(27) ? null : reader.GetString(27),
                NumberOfPreviousOwners = reader.IsDBNull(28) ? null : reader.GetUInt16(28),
                BatteryCapacityInKWh = reader.IsDBNull(29) ? null : reader.GetUInt32(29),
                RangeInKilometers = reader.IsDBNull(30) ? null : reader.GetUInt32(30),
                AverageFuelConsumptionInLitersPer100Km = reader.IsDBNull(31) ? null : reader.GetUInt32(31),
                AverageBatteryConsumptionInKWhPer100Km = reader.IsDBNull(32) ? null : reader.GetUInt16(32),
                MassInKg = reader.IsDBNull(33) ? null : reader.GetUInt32(33),
                MaximumLoadInKg = reader.IsDBNull(34) ? null : reader.GetUInt32(34),
                Directory = reader.IsDBNull(35) ? null : reader.GetString(35),
                PhotoKeys = reader.IsDBNull(36) ? null : reader.GetString(36).Split(VehicleSaleConfiguration.Separator)
            };
        }

        return null;
    }
}

public sealed record VehicleSaleFullDto(
    int Id,

    [property: Description("The display title of the sale listing.")]
    [property: MinLength(SaleTitle.MinLength), MaxLength(SaleTitle.MaxLength)]
    string Title,

    int SellerId,

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
    public IReadOnlyList<string>? PhotoKeys { get; init; }
}
