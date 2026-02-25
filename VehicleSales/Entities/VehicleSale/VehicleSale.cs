using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using VehicleSales.Entities.VehicleMake;

namespace VehicleSales.Entities.VehicleSale;

internal sealed class VehicleSale
{
    public int Id { get; }
    public required int SellerId { get; init; }
    public User.User? Seller { get; init; }
    public required Sale Sale { get; set; }
    public required VehicleDetails VehicleDetails { get; set; }
}

internal sealed record Sale
{
    public required SaleTitle Title { get; set; }
    public required SaleDescription Description { get; set; }
    public required Money SalePrice { get; set; }
    public required Location Location { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.InValidation;
    /// <summary>
    /// By default it's DateTimeOffset.Now, but it can be set to a 
    /// custom value for testing purposes or if the creation time needs 
    /// to be specified explicitly.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset? UpdatedAt { get; private set; }
}

internal sealed record VehicleDetails
{
    public required int VehicleModelId { get; set; }
    public VehicleModel? VehicleModel { get; set; }
    public uint? MileageInKilometers { get; set; }
    public uint? HorsePower { get; set; }
    public VehicleVersion? VehicleVersion { get; set; }
    public BodyType? BodyType { get; set; }
    public uint? EngineVolumeInCm3 { get; set; }
    public ColorName? ExteriorColor { get; set; }
    public ColorName? InteriorColor { get; set; }
    public FuelType? FuelType { get; set; }
    public VehicleManufacturingYear? VehicleManufacturingYear { get; set; }
    public NumberBetween1And9? VehicleNumberOfDoors { get; set; }
    public VehicleCondition? VehicleCondition { get; set; }
    public GearboxType? GearboxType { get; set; }
    public Side? SteeringWheelSide { get; set; }
    public DriveType? DriveType { get; set; }
    public ushort? NumberOfSeats { get; set; }
    public EmissionStandard? EmissionStandard { get; set; }
    public bool? HasServiceHistory { get; set; }
    public bool? HasAccidentHistory { get; set; }
    public VIN? Vin { get; set; }
    public ushort? NumberOfPreviousOwners { get; set; }
    public uint? BatteryCapacityInKWh { get; set; }
    public uint? RangeInKilometers { get; set; }
    public uint? AverageFuelConsumptionInLitersPer100Km { get; set; }
    public ushort? AverageBatteryConsumptionInKWhPer100Km { get; set; }
    public uint? MassInKg { get; set; }
    /// <summary>
    /// The maximum load capacity that the vehicle can support.
    /// </summary>
    public uint? MaximumLoadInKg { get; set; }
    // TODO: Add photo URLs as JSON array in one column.
}

internal sealed record SaleTitle
{
    public const int MaxLength = 100;
    public const int MinLength = 15;
    private static readonly string Error =
        $"Title must be between {MinLength} and {MaxLength} characters";

    public string Value { get; }

    private SaleTitle(string value)
    {
        Value = value;
    }

    public static Result<SaleTitle> Create(string? value) =>
        value?.Length >= MinLength && value.Length <= MaxLength
            ? Result.Success(new SaleTitle(value))
            : Result.Failure<SaleTitle>(Error);
}

internal sealed record SaleDescription
{
    public const int MaxLength = 5000;
    public const int MinLength = 100;
    private static readonly string Error =
        $"Description must be between {MinLength} and {MaxLength} characters";

    public string Value { get; }

    private SaleDescription(string value)
    {
        Value = value;
    }

    public static Result<SaleDescription> Create(string? value) =>
        value?.Length >= MinLength && value.Length <= MaxLength
            ? Result.Success(new SaleDescription(value))
            : Result.Failure<SaleDescription>(Error);
}

internal sealed record Money
{
    public uint AmountInCents { get; private set; }
    public Currency Currency { get; private set; }

    private Money(uint amountInCents, Currency currency)
    {
        AmountInCents = amountInCents;
        Currency = currency;
    }

    public static Result<Money> Create(uint? amountInCents, Currency currency) =>
        amountInCents >= 0
            ? new Money(amountInCents.Value, currency)
            : Result.Failure<Money>("Amount must be positive");
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Currency
{
    EUR,
    RON
}

internal sealed record Location(
    LocationName County,
    LocationName Locality);

internal sealed record LocationName
{
    public const int MinLength = 3;
    public const int MaxLength = 30;
    private static readonly string Error =
        $"String length must be between {MinLength} and {MaxLength}.";

    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName> Create(string? value) =>
        value?.Length >= MinLength
            ? Result.Success(new LocationName(value))
            : Result.Failure<LocationName>(Error);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum SaleStatus
{
    InValidation,
    Active,
    Sold,
    Deactivated,
    Expired
}

internal sealed record VehicleVersion
{
    public const int MaxLength = 100;
    public const int MinLength = 3;
    private static readonly string Error =
        $"String must be between {MinLength} and {MaxLength} characters";

    public string Value { get; }

    private VehicleVersion(string value)
    {
        Value = value;
    }

    public static Result<VehicleVersion> Create(string? value) =>
        value?.Length >= MinLength && value.Length <= MaxLength
            ? Result.Success(new VehicleVersion(value))
            : Result.Failure<VehicleVersion>(Error);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BodyType
{
    Sedan,
    Hatchback,
    SUV,
    Coupe,
    Convertible,
    Wagon,
    Van,
    Pickup
}

internal sealed record ColorName
{
    public const int MinLength = 3;
    public const int MaxLength = 30;
    private static readonly string Error =
        $"String length must be between {MinLength} and {MaxLength}.";

    public string Value { get; }

    private ColorName(string value)
    {
        Value = value;
    }

    public static Result<ColorName> Create(string? value) =>
        value?.Length >= MinLength
            ? Result.Success(new ColorName(value))
            : Result.Failure<ColorName>(Error);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FuelType
{
    Petrol,
    Diesel,
    Electric,
    Hybrid
}

internal sealed record VehicleManufacturingYear
{
    public ushort Value { get; }

    private VehicleManufacturingYear(ushort value)
    {
        Value = value;
    }

    public static Result<VehicleManufacturingYear> Create(ushort? value, int currentYear) =>
        value >= 1880 && value <= currentYear
            ? Result.Success(new VehicleManufacturingYear(value.Value))
            : Result.Failure<VehicleManufacturingYear>($"Value must be between 1880 and {currentYear}");
}

internal sealed record NumberBetween1And9
{
    public const int One = 1;
    public const int Nine = 9;
    private static readonly string Error =
        $"Value must be between {One} and {Nine}";

    public ushort Value { get; }

    private NumberBetween1And9(ushort value)
    {
        Value = value;
    }

    public static Result<NumberBetween1And9> Create(ushort? value) =>
        value >= One && value <= Nine
            ? Result.Success(new NumberBetween1And9(value.Value))
            : Result.Failure<NumberBetween1And9>(Error);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VehicleCondition
{
    New,
    Used
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GearboxType
{
    Manual,
    Automatic
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Side
{
    Left,
    Right,
    Middle
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DriveType
{
    FrontWheelDrive,
    RearWheelDrive,
    AllWheelDrive,
    FourWheelDrive
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmissionStandard
{
    EURO1, EURO2, EURO3, EURO4, EURO5, EURO6, EURO7
}

internal sealed record VIN
{
    public const int MinLength = 5;
    public const int MaxLength = 17;
    private static readonly string Error =
        $"Value must be between {MinLength} and {MaxLength}";

    public string Value { get; }

    private VIN(string value)
    {
        Value = value;
    }

    public static Result<VIN> Create(string? value) =>
        value?.Length >= MinLength && value?.Length <= MaxLength
            ? Result.Success(new VIN(value))
            : Result.Failure<VIN>(Error);
}
