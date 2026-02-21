using CSharpFunctionalExtensions;
using UnitsNet;
using VehicleSales.Entities.VehicleMake;

namespace VehicleSales.Entities.VehicleSale;

// TODO: properties to add
//Listing Metadata
//•	CreatedAt / UpdatedAt(DateTimeOffset) — when the listing was posted / modified
//•	ListingStatus — e.g., Active, Sold, Expired
//•	Location — city/region where the vehicle is sold
//Vehicle Details
//•	DriveType — FWD, RWD, AWD/4WD — very commonly filtered by buyers
//•	NumberOfSeats — important for families
//•	InteriorColor — separate from exterior color
//•	EmissionStandard — e.g., Euro4, Euro5, Euro6
//•	VIN — Vehicle Identification Number, useful for validation
//History & Condition
//•	NumberOfPreviousOwners — strong buying signal
//•	HasServiceHistory(bool) — very relevant for used vehicles
//•	HasAccidentHistory(bool)
//EV-Specific(since you have Electric/Hybrid in FuelType)
//•	BatteryCapacityInKwh(Energy from UnitsNet)
//•	RangeInKilometers(Length from UnitsNet)

// TODO: Group properties into ListingMetadata, VehicleDetails, History, etc.
// TODO: Put immutable properties at the top of the entity.
// TODO: Add types of vehicles: family car, truck, jeep, tractor, motorbike
internal sealed class VehicleSale(
    int id,
    SaleTitle title,
    SaleDescription description,
    VehicleModel vehicleModel,
    string version,
    Power horsePower,
    BodyType bodyType,
    Length mileageInKilometers,
    Volume engineVolume,
    MinLength3String exteriorColor,
    MinLength3String interiorColor,
    FuelType fuelType,
    Money salePrice,
    VehicleManufacturingYear vehicleManufacturingYear,
    NumberBetween1And9 vehicleNumberOfDoors,
    VehicleCondition vehicleCondition,
    GearboxType gearboxType,
    Side wheelSide,
    Location location,
    DateTimeOffset createdAt,
    DateTimeOffset updatedAt,
    SaleStatus status,
    VehicleDriveType driveType,
    NumberBetween1And9 numberOfSeats,
    EmissionStandard emissionStandard,
    VIN? vin,
    ushort? numberOfPreviousOwners,
    bool hasServiceHistory,
    bool hasAccidentHistory,
    ushort batteryCapacity,
    Length rangeInKilometers,
    FuelEfficiency averageFuelConsumptionPer100Km,
    ushort? averageBatteryConsumptionPer100Km)
{
    public int Id { get; init; } = id;
    public SaleTitle Title { get; set; } = title;
    public SaleDescription Description { get; set; } = description;
    public VehicleModel VehicleModel { get; set; } = vehicleModel;
    public string Version { get; set; } = version;
    public Power HorsePower { get; set; } = horsePower;
    public BodyType BodyType { get; set; } = bodyType;
    public Length MileageInKilometers { get; set; } = mileageInKilometers;
    public Volume EngineVolume { get; set; } = engineVolume;
    public MinLength3String ExteriorColor { get; set; } = exteriorColor;
    public MinLength3String InteriorColor { get; set; } = interiorColor;
    // TODO: create a discriminated union for fuel type specific properties (e.g., battery capacity for electric vehicles)
    public FuelType FuelType { get; set; } = fuelType;
    public Money SalePrice { get; set; } = salePrice;
    public VehicleManufacturingYear VehicleManufacturingYear { get; set; } = vehicleManufacturingYear;
    public NumberBetween1And9 VehicleNumberOfDoors { get; set; } = vehicleNumberOfDoors;
    public VehicleCondition VehicleCondition { get; set; } = vehicleCondition;
    public GearboxType GearboxType { get; set; } = gearboxType;
    public Side WheelSide { get; set; } = wheelSide;
    public Location Location { get; set; } = location;
    public DateTimeOffset CreatedAt { get; private set; } = createdAt;
    public DateTimeOffset UpdatedAt { get; set; } = updatedAt;
    public SaleStatus Status { get; set; } = status;
    public VehicleDriveType DriveType { get; set; } = driveType;
    public NumberBetween1And9 NumberOfSeats { get; set; } = numberOfSeats;
    public EmissionStandard EmissionStandard { get; set; } = emissionStandard;
    public VIN? Vin { get; set; } = vin;
    public ushort? NumberOfPreviousOwners { get; set; } = numberOfPreviousOwners;
    public bool HasServiceHistory { get; set; } = hasServiceHistory;
    public bool HasAccidentHistory { get; set; } = hasAccidentHistory;
    public ushort BatteryCapacity { get; set; } = batteryCapacity;
    public Length RangeInKilometers { get; set; } = rangeInKilometers;
    public FuelEfficiency AverageFuelConsumptionPer100Km { get; set; } = averageFuelConsumptionPer100Km;
    public ushort? AverageBatteryConsumptionPer100Km { get; set; } = averageBatteryConsumptionPer100Km;
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

internal enum VehicleDriveType
{
    FWD, RWD, AWD, FourWD
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
