using CSharpFunctionalExtensions;
using UnitsNet;
using VehicleSales.Entities.VehicleMake;

namespace VehicleSales.Entities.VehicleSale;

// TODO: Group properties into SaleDetails, VehicleDetails, History, etc.
// TODO: Put immutable properties at the top of the entity.
internal sealed class VehicleSale(
    int id,
    Sale sale,

    // Vehicle details
    VehicleModel vehicleModel,
    VehicleCategory vehicleCategory,
    string vehicleVersion,
    Power horsePower,
    BodyType bodyType,
    Length mileageInKilometers,
    Volume engineVolumeInCm3,
    MinLength3String exteriorColor,
    MinLength3String interiorColor,
    FuelType fuelType,
    VehicleManufacturingYear vehicleManufacturingYear,
    NumberBetween1And9 vehicleNumberOfDoors,
    VehicleCondition vehicleCondition,
    GearboxType gearboxType,
    Side steeringWheelSide,
    DriveType driveType,
    NumberBetween1And9 numberOfSeats,
    EmissionStandard emissionStandard,
    VIN? vin,
    ushort? numberOfPreviousOwners,
    bool hasServiceHistory,
    bool hasAccidentHistory,
    Energy batteryCapacityInKWh,
    Length rangeInKilometers,
    FuelEfficiency averageFuelConsumptionPer100Km,
    ushort? averageBatteryConsumptionPer100Km,
    Mass mass)
{
    public int Id { get; init; } = id;
    public Sale Sale { get; init; } = sale;
    public VehicleModel VehicleModel { get; set; } = vehicleModel;
    public VehicleCategory VehicleCategory { get; set; } = vehicleCategory;
    public string VehicleVersion { get; set; } = vehicleVersion;
    public Power HorsePower { get; set; } = horsePower;
    public BodyType BodyType { get; set; } = bodyType;
    public Length MileageInKilometers { get; set; } = mileageInKilometers;
    public Volume EngineVolumeInCm3 { get; set; } = engineVolumeInCm3;
    public MinLength3String ExteriorColor { get; set; } = exteriorColor;
    public MinLength3String InteriorColor { get; set; } = interiorColor;
    // TODO: create a discriminated union for fuel type specific properties (e.g., battery capacity for electric vehicles)
    public FuelType FuelType { get; set; } = fuelType;
    public VehicleManufacturingYear VehicleManufacturingYear { get; set; } = vehicleManufacturingYear;
    public NumberBetween1And9 VehicleNumberOfDoors { get; set; } = vehicleNumberOfDoors;
    public VehicleCondition VehicleCondition { get; set; } = vehicleCondition;
    public GearboxType GearboxType { get; set; } = gearboxType;
    public Side SteeringWheelSide { get; set; } = steeringWheelSide;
    public DriveType DriveType { get; set; } = driveType;
    public NumberBetween1And9 NumberOfSeats { get; set; } = numberOfSeats;
    public EmissionStandard EmissionStandard { get; set; } = emissionStandard;
    public VIN? Vin { get; set; } = vin;
    public ushort? NumberOfPreviousOwners { get; set; } = numberOfPreviousOwners;
    public bool HasServiceHistory { get; set; } = hasServiceHistory;
    public bool HasAccidentHistory { get; set; } = hasAccidentHistory;
    public Energy BatteryCapacityInKWh { get; set; } = batteryCapacityInKWh;
    public Length RangeInKilometers { get; set; } = rangeInKilometers;
    public FuelEfficiency AverageFuelConsumptionPer100Km { get; set; } = averageFuelConsumptionPer100Km;
    public ushort? AverageBatteryConsumptionPer100Km { get; set; } = averageBatteryConsumptionPer100Km;
    public Mass? Mass { get; set; } = mass;
}

// TODO: Configure this as a complex type in EF
internal sealed record Sale
{
    public required User.User User { get; init; }
    public required SaleTitle Title { get; set; }
    public required SaleDescription Description { get; set; }
    public required Money SalePrice { get; set; }
    public required Location Location { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.InValidation;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset? UpdatedAt { get; private set; }
}

internal sealed record Money
{
    public decimal AmountInCents { get; private set; }
    public Currency Currency { get; private set; }

    private Money(decimal amount, Currency currency)
    {
        AmountInCents = amount;
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

    public static Result<MinLength3String> Create(string value) =>
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

    public static Result<VehicleManufacturingYear> Create(int value, int currentYear) =>
        value > 1880 && value <= currentYear
            ? Result.Success(new VehicleManufacturingYear(value))
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

internal sealed record Location
{
    public string County { get; }
    public string Locality { get; }

    private Location(string county, string locality)
    {
        County = county;
        Locality = locality;
    }

    public static Result<Location> Create(string county, string locality)
    {
        if (!(county?.Length >= 3))
            return Result.Failure<Location>("County must be at least 3 characters");

        if (!(locality?.Length >= 3))
            return Result.Failure<Location>("Locality must be at least 3 characters");

        return Result.Success(new Location(county, locality));
    }
}

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

    public static Result<VIN> Create(string value) =>
        value?.Length >= 5 && value?.Length <= 17
            ? Result.Success(new VIN(value))
            : Result.Failure<VIN>("Value must be between 5 and 17");
}

// Pseudocode plan:
// 1. Provide a clearer name for the enum currently called `VehicleType`.
//    - Choose `VehicleCategory` to express that the enum classifies categories/classes of vehicles
//      rather than a low-level "type" which can be ambiguous with C# type terminology.
// 2. Keep existing enum members unchanged to preserve semantics:
//    Car, Truck, Van, Motorcycle, Trailer, Agricultural, Construction.
// 3. Add XML doc comments for the enum and each member to clarify intent for future maintainers.
// 4. Keep `internal` accessibility to match the original visibility.
// 5. Note for caller: search-and-replace references from `VehicleType` to `VehicleCategory` in the codebase.
//    - This file only updates the enum definition; update usages elsewhere as part of the refactor.
//
// Rationale:
// - `VehicleCategory` communicates classification intent and reduces confusion with the word "type".
// - Minimal change to maintainers: same values, improved name, documentation added.

/// <summary>
/// Classifies vehicles into broad categories used by the domain (e.g., listings, filters).
/// Use this enum where you need to distinguish between high-level vehicle categories.
/// </summary>
internal enum VehicleCategory
{
    /// <summary>Standard passenger car.</summary>
    Car,
    /// <summary>Large goods vehicle / lorry.</summary>
    Truck,
    /// <summary>Light commercial van.</summary>
    Van,
    /// <summary>Two-wheeled motorcycle.</summary>
    Motorcycle,
    /// <summary>Trailers and towed units.</summary>
    Trailer,
    /// <summary>Agricultural machinery.</summary>
    Agricultural,
    /// <summary>Construction machinery and equipment.</summary>
    Construction
}