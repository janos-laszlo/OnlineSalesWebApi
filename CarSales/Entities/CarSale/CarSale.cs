using CSharpFunctionalExtensions;
using UnitsNet;

namespace CarSales.Entities.CarSale;

internal sealed class CarSale(
    int id,
    CarSaleTitle title,
    CarSaleDescription description,
    Volume engineVolume,
    Money price)
{
    public int Id { get; init; } = id;
    public CarSaleTitle Title { get; set; } = title;
    public CarSaleDescription Description { get; set; } = description;
    public Volume EngineVolume { get; set; } = engineVolume;
    public Money Price { get; set; } = price;
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

    public static Result<Money> Create(int amountInCents, Currency currency)
    {
        if (amountInCents < 0)
            return Result.Failure<Money>("Amount cannot be negative");

        return new Money(amountInCents, currency);
    }
}

internal enum Currency
{
    EUR,
    RON
}

/// <summary>
/// Base record for strings with length constraints.
/// </summary>
internal abstract record BoundedString
{
    public string Value { get; init; }

    protected BoundedString(string value)
    {
        Value = value;
    }

    protected static Result Validate(string value, int minLength, int maxLength, string errorMessage) =>
        string.IsNullOrWhiteSpace(value) || value.Length < minLength || value.Length > maxLength
            ? Result.Failure(errorMessage)
            : Result.Success();
}

/// <summary>
/// Represents the title of a car sale, ensuring it meets specific length requirements.
/// </summary>
/// <remarks>The title must be between 15 and 64 characters in length. Attempting to create a CarSaleTitle with an
/// invalid title will result in an ArgumentException. Use the Create method to instantiate a CarSaleTitle
/// safely.</remarks>
internal sealed record CarSaleTitle : BoundedString
{
    private const int MinLength = 15;
    private const int MaxLength = 64;
    private const string InvalidTitleErrorMessage = "Title must be between 15 and 64 characters";

    private CarSaleTitle(string value) : base(value)
    {
    }

    public static Result<CarSaleTitle> Create(string value)
    {
        var validationResult = Validate(value, MinLength, MaxLength, InvalidTitleErrorMessage);
        return validationResult.IsSuccess
            ? Result.Success(new CarSaleTitle(value))
            : Result.Failure<CarSaleTitle>(validationResult.Error);
    }
}

/// <summary>
/// Represents the description of a car sale, ensuring it meets specific length requirements.
/// </summary>
/// <remarks>The description must be between 100 and 1000 characters in length. Attempting to create a CarSaleDescription with an
/// invalid description will result in an ArgumentException. Use the Create method to instantiate a CarSaleDescription
/// safely.</remarks>
internal sealed record CarSaleDescription : BoundedString
{
    private const int MinLength = 100;
    private const int MaxLength = 1000;
    private const string InvalidDescriptionErrorMessage = "Description must be between 100 and 1000 characters";

    private CarSaleDescription(string value) : base(value)
    {
    }

    public static Result<CarSaleDescription> Create(string value)
    {
        var validationResult = Validate(value, MinLength, MaxLength, InvalidDescriptionErrorMessage);
        return validationResult.IsSuccess
            ? Result.Success(new CarSaleDescription(value))
            : Result.Failure<CarSaleDescription>(validationResult.Error);
    }
}
