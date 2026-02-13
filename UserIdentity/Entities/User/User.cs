using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;

namespace UserIdentity.Entities;

internal partial class User
{
    public int Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public ProfileType? ProfileType { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Cui { get; private set; }
    public string? CompanyName { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public string? Address { get; private set; }
    public string? County { get; private set; }
    public string? Locality { get; private set; }
    public IReadOnlyList<string>? PhoneNumbers { get; private set; }
    public Profile? Profile
    {
        get => ProfileType switch
        {
            null => NoProfile
                .Create(Email, PhoneNumbers)
                .Value,
            Entities.ProfileType.Regular => RegularProfile
                .Create(Email, FirstName, LastName, PhoneNumbers)
                .Value,
            Entities.ProfileType.Dealer => DealerProfile
                .Create(Email, Cui, CompanyName, RegistrationNumber, Address, County, Locality, PhoneNumbers)
                .Value,
            _ => throw new InvalidOperationException("Unknown profile type")
        };
        set
        {
            switch (value)
            {
                case RegularProfile regular:
                    Email = regular.Email;
                    ProfileType = Entities.ProfileType.Regular;
                    FirstName = regular.FirstName;
                    LastName = regular.LastName;
                    Cui = null;
                    CompanyName = null;
                    RegistrationNumber = null;
                    Address = null;
                    County = null;
                    Locality = null;
                    PhoneNumbers = regular.PhoneNumbers;
                    break;
                case DealerProfile dealer:
                    Email = dealer.Email;
                    ProfileType = Entities.ProfileType.Dealer;
                    FirstName = null;
                    LastName = null;
                    Cui = dealer.Cui;
                    CompanyName = dealer.CompanyName;
                    RegistrationNumber = dealer.RegistrationNumber;
                    Address = dealer.Address;
                    County = dealer.County;
                    Locality = dealer.Locality;
                    PhoneNumbers = dealer.PhoneNumbers;
                    break;
                case NoProfile:
                    Email = value.Email;
                    ProfileType = null;
                    FirstName = null;
                    LastName = null;
                    Cui = null;
                    CompanyName = null;
                    RegistrationNumber = null;
                    Address = null;
                    County = null;
                    Locality = null;
                    PhoneNumbers = value.PhoneNumbers;
                    break;
                default:
                    throw new InvalidOperationException("Unknown profile type");
            }
        }
    }

    [GeneratedRegex("(?=.*[a-z])(?=.*[A-Z])")]
    private static partial Regex charactersRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex numbersRegex();

    private User(
        int id,
        string email,
        string passwordHash,
        bool emailConfirmed,
        DateTimeOffset createdAt,
        ProfileType? profileType,
        string? firstName,
        string? lastName,
        string? cui,
        string? companyName,
        string? registrationNumber,
        string? address,
        string? county,
        string? locality,
        IReadOnlyList<string>? phoneNumbers)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        EmailConfirmed = emailConfirmed;
        CreatedAt = createdAt;
        this.ProfileType = profileType;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.Cui = cui;
        this.CompanyName = companyName;
        this.RegistrationNumber = registrationNumber;
        this.Address = address;
        this.County = county;
        this.Locality = locality;
        this.PhoneNumbers = phoneNumbers;
    }

    private User(string email, string hashedPassword) :
        this(0, email, hashedPassword, false, DateTimeOffset.UtcNow,
            null, null, null, null, null, null, null, null, null, null)
    {
    }

    public static Result<User> Create(string email, string password) => 
        Profile.Validate(email)
            .Bind(() =>
                string.IsNullOrWhiteSpace(password) ||
                password.Length < 8 ||
                password.Length > 32 ||
                !charactersRegex().IsMatch(password) ||
                !numbersRegex().IsMatch(password)
                    ? Result.Failure<User>("Password must be between 8 and 32 characters long, and contain upper and lowercase characters and numbers")
                    : Result.Success())
            .Map(() =>
            {
                var passwordhasher = new PasswordHasher<User>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                // In this context, passing null is acceptable because we are not using any user-specific data for hashing.
                var hashedPassword = passwordhasher.HashPassword(null, password);
                return new User(email.ToLowerInvariant(), hashedPassword);
            });

    internal bool ConfirmEmail(DateTime confirmedAt)
    {
        if (this.CreatedAt + TimeSpan.FromDays(7) < confirmedAt)
            return false;
        EmailConfirmed = true;
        return true;
    }
}

internal abstract partial record Profile(string Email, IReadOnlyList<string>? PhoneNumbers)
{
    [GeneratedRegex(@"^\S+@\S+\.\S+$")]
    private static partial Regex emailRegex();

    public static Result Validate(string? email) =>
        Result.FailureIf(
            email is null || !emailRegex().IsMatch(email),
            "Invalid email format");

    protected static Result Validate(string? email, IReadOnlyList<string>? phoneNumbers) =>
        Validate(email)
            .Bind(() => phoneNumbers == null ||
                phoneNumbers.Count == 0 ||
                phoneNumbers.Any(p => string.IsNullOrWhiteSpace(p))
                    ? Result.Failure("Invalid phone number/s provided")
                    : Result.Success());
};

internal sealed record NoProfile : Profile
{
    private NoProfile(string email, IReadOnlyList<string>? phoneNumbers) : base(email, phoneNumbers)
    {
    }

    internal static Result<NoProfile> Create(string? email, IReadOnlyList<string>? phoneNumbers) =>
        Validate(email)
            .Bind(() => phoneNumbers?.Any(p => string.IsNullOrWhiteSpace(p)) == true
                ? Result.Failure<NoProfile>("Invalid phone number/s provided")
                : Result.Success())
            .Map(() => new NoProfile(email!, phoneNumbers));
}

internal sealed record RegularProfile : Profile
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    private RegularProfile(string email, string firstName, string lastName, IEnumerable<string> phoneNumbers)
        : base(email, [.. phoneNumbers])
    {
        FirstName = firstName;
        LastName = lastName;
    }

    internal static Result<RegularProfile> Create(
        string? email,
        string? firstName,
        string? lastName,
        IReadOnlyList<string>? phoneNumbers)
    =>
        Validate(email, phoneNumbers)
            .Bind(() =>
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName)
                    ? (Result)Result.Failure<RegularProfile>("First name and last name must be provided")
                    : Result.Success())
            .Map(() => new RegularProfile(email!, firstName!, lastName!, phoneNumbers!));
}

internal sealed record DealerProfile : Profile
{
    public string Cui { get; private set; }
    public string CompanyName { get; private set; }
    public string RegistrationNumber { get; private set; }
    public string Address { get; private set; }
    public string County { get; private set; }
    public string Locality { get; private set; }

    private DealerProfile(
        string email,
        string cui,
        string companyName,
        string registrationNumber,
        string address,
        string county,
        string locality,
        IEnumerable<string> phoneNumbers) : base(email, [.. phoneNumbers])
    {
        Cui = cui;
        CompanyName = companyName;
        RegistrationNumber = registrationNumber;
        Address = address;
        County = county;
        Locality = locality;
    }

    internal static Result<DealerProfile> Create(
        string? email,
        string? cui,
        string? companyName,
        string? registrationNumber,
        string? address,
        string? county,
        string? locality,
        IReadOnlyList<string>? phoneNumbers)
    =>
        Validate(email, phoneNumbers)
            .Bind(() =>
                string.IsNullOrWhiteSpace(cui) ||
                cui.Length != 8 ||
                !cui.All(char.IsDigit) ||
                string.IsNullOrWhiteSpace(companyName) ||
                string.IsNullOrWhiteSpace(registrationNumber) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(county) ||
                string.IsNullOrWhiteSpace(locality)
                    ? (Result)Result.Failure<DealerProfile>("All company details must be provided for dealers")
                    : Result.Success())
            .Map(() => new DealerProfile(email!, cui!, companyName!, registrationNumber!, address!, county!, locality!, phoneNumbers!));
}

internal static class ProfileExtensions
{
    public static T Match<T>(
        this Profile? profile,
        Func<RegularProfile, T> regularFunc,
        Func<DealerProfile, T> dealerFunc,
        Func<NoProfile, T> noProfileFunc)
    =>
        profile switch
        {
            RegularProfile regular => regularFunc(regular),
            DealerProfile dealer => dealerFunc(dealer),
            NoProfile noProfile => noProfileFunc(noProfile),
            _ => throw new InvalidOperationException("Unknown profile type"),
        };
}

internal enum ProfileType
{
    Regular,
    Dealer
}
