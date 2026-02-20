using CarSales.Entities.CarMake;
using CSharpFunctionalExtensions;
using UnitsNet;

namespace CarSales.Entities.CarSale;

// TODO: properties to add
//Listing Metadata
//•	CreatedAt / UpdatedAt(DateTimeOffset) — when the listing was posted / modified
//•	ListingStatus — e.g., Active, Sold, Expired
//•	Location — city/region where the car is sold
//Vehicle Details
//•	DriveType — FWD, RWD, AWD/4WD — very commonly filtered by buyers
//•	NumberOfSeats — important for families
//•	InteriorColor — separate from exterior color
//•	EmissionStandard — e.g., Euro4, Euro5, Euro6
//•	VIN — Vehicle Identification Number, useful for validation
//History & Condition
//•	NumberOfPreviousOwners — strong buying signal
//•	HasServiceHistory(bool) — very relevant for used cars
//•	HasAccidentHistory(bool)
//EV-Specific(since you have Electric/Hybrid in FuelType)
//•	BatteryCapacityInKwh(Energy from UnitsNet)
//•	RangeInKilometers(Length from UnitsNet)
internal sealed class CarSale(
    int id,
    CarSaleTitle title,
    CarSaleDescription description,
    CarModel carModel,
    string version,
    Power horsePower,
    BodyType bodyType,
    Length mileageInKilometers,
    Volume engineVolume,
    MinLength3String exteriorColor,
    MinLength3String interiorColor,
    FuelType fuelType,
    Money salePrice,
    CarManufacturingYear carManufacturingYear,
    CarNumberOfDoors carNumberOfDoors,
    CarCondition carCondition,
    GearboxType gearboxType,
    Side wheelSide)
{
    public int Id { get; init; } = id;
    public CarSaleTitle Title { get; set; } = title;
    public CarSaleDescription Description { get; set; } = description;
    public CarModel CarModel { get; set; } = carModel;
    public string Version { get; set; } = version;
    public Power HorsePower { get; set; } = horsePower;
    public BodyType BodyType { get; set; } = bodyType;
    public Length MileageInKilometers { get; set; } = mileageInKilometers;
    public Volume EngineVolume { get; set; } = engineVolume;
    public MinLength3String ExteriorColor { get; set; } = exteriorColor;
    public MinLength3String InteriorColor { get; set; } = interiorColor;
    // TODO: create a discriminated union for fuel type specific properties (e.g., battery capacity for electric cars)
    public FuelType FuelType { get; set; } = fuelType;
    public Money SalePrice { get; set; } = salePrice;
    public CarManufacturingYear CarManufacturingYear { get; set; } = carManufacturingYear;
    public CarNumberOfDoors CarNumberOfDoors { get; set; } = carNumberOfDoors;
    public CarCondition CarCondition { get; set; } = carCondition;
    public GearboxType GearboxType { get; set; } = gearboxType;
    public Side WheelSide { get; set; } = wheelSide;
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
/// Represents the title of a car sale, ensuring it meets specific length requirements.
/// </summary>
/// <remarks>The title must be between 15 and 64 characters in length. Attempting to create a CarSaleTitle with an
/// invalid title will result in an ArgumentException. Use the Create method to instantiate a CarSaleTitle
/// safely.</remarks>
internal sealed record CarSaleTitle
{
    public string Value { get; }

    private CarSaleTitle(string value)
    {
        Value = value;
    }

    public static Result<CarSaleTitle> Create(string? value) =>
        value?.Length >= 15 && value.Length <= 50
            ? Result.Success(new CarSaleTitle(value))
            : Result.Failure<CarSaleTitle>("Title must be between 15 and 64 characters");
}

/// <summary>
/// Represents the description of a car sale, ensuring it meets specific length requirements.
/// </summary>
/// <remarks>The description must be between 100 and 1000 characters in length. Attempting to create a CarSaleDescription with an
/// invalid description will result in an ArgumentException. Use the Create method to instantiate a CarSaleDescription
/// safely.</remarks>
internal sealed record CarSaleDescription
{
    public string Value { get; }

    private CarSaleDescription(string value)
    {
        Value = value;
    }

    public static Result<CarSaleDescription> Create(string? value) =>
        value?.Length >= 100 && value.Length <= 1000
            ? Result.Success(new CarSaleDescription(value))
            : Result.Failure<CarSaleDescription>("Description must be between 100 and 1000 characters");
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


internal sealed record CarManufacturingYear
{
    public int Value { get; }

    private CarManufacturingYear(int value)
    {
        Value = value;
    }

    public static Result<CarManufacturingYear> Create(int value, int currentYear) =>
        value > 1880 && value <= currentYear
            ? Result.Success(new CarManufacturingYear(value))
            : Result.Failure<CarManufacturingYear>("Value must be greater than 1880");
}



internal sealed record CarNumberOfDoors
{
    public int Value { get; }

    private CarNumberOfDoors(int value)
    {
        Value = value;
    }

    public static Result<CarNumberOfDoors> Create(int? value) =>
        value > 0 && value < 10
            ? Result.Success(new CarNumberOfDoors(value.Value))
            : Result.Failure<CarNumberOfDoors>("Value must be between 1 and 9");
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

internal enum CarCondition
{
    New,
    Used
}