using CSharpFunctionalExtensions;

namespace VehicleSales.Entities.VehicleSale;

internal sealed record DirectoryName
{
    public const int MinLength = 1;
    public const int MaxLength = 32;
    private static readonly string Error =
        $"Value must be between {MinLength} and {MaxLength} characters.";

    public string Value { get; }

    private DirectoryName(string value)
    {
        Value = value;
    }

    public static Result<DirectoryName> Create(string? value) =>
        value?.Length >= MinLength && value?.Length <= MaxLength
            ? Result.Success(new DirectoryName(value))
            : Result.Failure<DirectoryName>(Error);
}

internal sealed record ObjectKeyName
{
    public const int MinLength = 1;
    public const int MaxLength = 10;
    private static readonly string Error =
        $"Value must be between {MinLength} and {MaxLength} characters.";

    public string Value { get; }

    private ObjectKeyName(string value)
    {
        Value = value;
    }

    public static Result<ObjectKeyName> Create(string? value) =>
        value?.Length >= MinLength && value?.Length <= MaxLength
            ? Result.Success(new ObjectKeyName(value))
            : Result.Failure<ObjectKeyName>(Error);
}
