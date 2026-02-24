using CSharpFunctionalExtensions;
using VehicleSales.Entities.VehicleMake;

namespace VehicleSales.Entities.VehicleSale;

internal sealed class VehicleSale
{
    public int Id { get; }
    public required int SellerId { get; init; }
    public required User.User Seller { get; init; }
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
    public required VehicleModel VehicleModel { get; set; }
    public uint? MileageInKilometers { get; set; }
    public uint? HorsePower { get; set; }
    public string? VehicleVersion { get; set; }
    public BodyType? BodyType { get; set; }
    public uint? EngineVolumeInCm3 { get; set; }
    public MinLength3String? ExteriorColor { get; set; }
    public MinLength3String? InteriorColor { get; set; }
    public FuelType? FuelType { get; set; }
    public VehicleManufacturingYear? VehicleManufacturingYear { get; set; }
    public NumberBetween1And9? VehicleNumberOfDoors { get; set; }
    public VehicleCondition? VehicleCondition { get; set; }
    public GearboxType? GearboxType { get; set; }
    public Side? SteeringWheelSide { get; set; }
    public DriveType? DriveType { get; set; }
    public NumberBetween1And9? NumberOfSeats { get; set; }
    public EmissionStandard? EmissionStandard { get; set; }
    public bool? HasServiceHistory { get; set; }
    public bool? HasAccidentHistory { get; set; }
    public VIN? Vin { get; set; }
    public ushort? NumberOfPreviousOwners { get; set; }
    public uint? BatteryCapacityInKWh { get; set; }
    public uint? RangeInKilometers { get; set; }
    public uint? AverageFuelConsumptionInLitersPer100Km { get; set; }
    public ushort? AverageBatteryConsumptionInKWhPer100Km { get; set; }
    public uint? Mass { get; set; }
    /// <summary>
    /// The maximum load capacity that the vehicle can support.
    /// </summary>
    public uint? MaximumLoad { get; set; }
}

internal sealed record Money
{
    public int AmountInCents { get; private set; }
    public Currency Currency { get; private set; }

    private Money(int amountInCents, Currency currency)
    {
        AmountInCents = amountInCents;
        Currency = currency;
    }

    public static Result<Money> Create(int? amountInCents, Currency currency) =>
        amountInCents >= 0
            ? new Money(amountInCents.Value, currency)
            : Result.Failure<Money>("Amount must be positive");
}

internal enum Currency
{
    EUR,
    RON
}

/// <summary>
/// Represents the title of a vehicle sale, ensuring it meets specific length requirements.
/// </summary>
/// <remarks>The title must be between 15 and 64 characters in length. Attempting to create a SaleTitle with an
/// invalid title will result in an ArgumentException. Use the Create method to instantiate a SaleTitle
/// safely.</remarks>
internal sealed record SaleTitle
{
    public string Value { get; }

    private SaleTitle(string value)
    {
        Value = value;
    }

    public static Result<SaleTitle> Create(string? value) =>
        value?.Length >= 15 && value.Length <= 50
            ? Result.Success(new SaleTitle(value))
            : Result.Failure<SaleTitle>("Title must be between 15 and 64 characters");
}

/// <summary>
/// Represents the description of a vehicle sale, ensuring it meets specific length requirements.
/// </summary>
/// <remarks>The description must be between 100 and 1000 characters in length. Attempting to create a SaleDescription with an
/// invalid description will result in an ArgumentException. Use the Create method to instantiate a SaleDescription
/// safely.</remarks>
internal sealed record SaleDescription
{
    public string Value { get; }

    private SaleDescription(string value)
    {
        Value = value;
    }

    public static Result<SaleDescription> Create(string? value) =>
        value?.Length >= 100 && value.Length <= 1000
            ? Result.Success(new SaleDescription(value))
            : Result.Failure<SaleDescription>("Description must be between 100 and 1000 characters");
}

internal enum FuelType
{
    Petrol,
    Diesel,
    Electric,
    Hybrid
}

internal enum BodyType
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

/// <summary>
/// Represents a string value that must contain at least three characters.
/// </summary>
/// <remarks>Use this type to enforce a minimum length constraint on string values. Instances can only be created
/// through the provided factory method, which ensures the value meets the required length.</remarks>
internal sealed record MinLength3String
{
    public string Value { get; }

    private MinLength3String(string value)
    {
        Value = value;
    }

    public static Result<MinLength3String> Create(string? value) =>
        value?.Length >= 3
            ? Result.Success(new MinLength3String(value))
            : Result.Failure<MinLength3String>("Value must be at least 3 characters");
}

internal sealed record VehicleManufacturingYear
{
    public int Value { get; }

    private VehicleManufacturingYear(int value)
    {
        Value = value;
    }

    public static Result<VehicleManufacturingYear> Create(int? value, int currentYear) =>
        value > 1880 && value <= currentYear
            ? Result.Success(new VehicleManufacturingYear(value.Value))
            : Result.Failure<VehicleManufacturingYear>("Value must be greater than 1880");
}

internal sealed record NumberBetween1And9
{
    public int Value { get; }

    private NumberBetween1And9(int value)
    {
        Value = value;
    }

    public static Result<NumberBetween1And9> Create(int? value) =>
        value > 0 && value < 10
            ? Result.Success(new NumberBetween1And9(value.Value))
            : Result.Failure<NumberBetween1And9>("Value must be between 1 and 9");
}

internal enum GearboxType
{
    Manual,
    Automatic
}

internal enum Side
{
    Left,
    Right,
    Middle
}

internal enum VehicleCondition
{
    New,
    Used
}

internal sealed record Location(
    MinLength3String County,
    MinLength3String Locality);

internal enum SaleStatus
{
    InValidation,
    Active,
    Sold,
    Deactivated,
    Expired
}

/// <summary>
/// Describes the vehicle's drive-wheel configuration (which wheels receive power).
/// Use this enum to specify the drivetrain layout for listings, filters and domain logic.
/// </summary>
internal enum DriveType
{
    /// <summary>Front-wheel drive: the engine drives the front wheels.</summary>
    FrontWheelDrive,

    /// <summary>Rear-wheel drive: the engine drives the rear wheels.</summary>
    RearWheelDrive,

    /// <summary>All-wheel drive: power can be distributed to all wheels as needed.</summary>
    AllWheelDrive,

    /// <summary>Four-wheel drive: typically selectable 4x4 drive for off-road or heavy-duty use.</summary>
    FourWheelDrive
}

internal enum EmissionStandard
{
    EURO1, EURO2, EURO3, EURO4, EURO5, EURO6, EURO7
}

internal sealed record VIN
{
    public string Value { get; }

    private VIN(string value)
    {
        Value = value;
    }

    public static Result<VIN> Create(string? value) =>
        value?.Length >= 5 && value?.Length <= 17
            ? Result.Success(new VIN(value))
            : Result.Failure<VIN>("Value must be between 5 and 17");
}
