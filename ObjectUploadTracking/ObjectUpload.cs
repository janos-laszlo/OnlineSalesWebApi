using CSharpFunctionalExtensions;

namespace ObjectUploadTracking;

public sealed class ObjectUpload()
{
    public int Id { get; }
    public required string Module { get; init; }
    public required int EntityId { get; init; }
    public required DirectoryName Directory { get; init; }
    /// <summary>
    /// The concatenation of the directory and object keys should not exceed
    /// <see cref="ObjectUploadConfiguration.ObjectKeysMaxLength"/> characters.
    /// </summary>
    public required IReadOnlyList<ObjectKeyName> ObjectKeys { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}

public sealed record DirectoryName
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

public sealed record ObjectKeyName
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
