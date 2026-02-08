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
    private ProfileType? profileType;
    private string? firstName;
    private string? lastName;
    private string? cui;
    private string? companyName;
    private string? registrationNumber;
    private string? address;
    private string? county;
    private string? locality;
    private IReadOnlyList<string>? phoneNumbers;
    public Profile? Profile
    {
        get
        {
            return profileType switch
            {
                ProfileType.Regular => RegularProfile
                    .Create(firstName, lastName, phoneNumbers)
                    .Value,
                ProfileType.Dealer => DealerProfile
                    .Create(cui, companyName, registrationNumber, address, county, locality, phoneNumbers)
                    .Value,
                _ => throw new InvalidOperationException("Unknown profile type")
            };
        }
        set
        {
            switch (value)
            {
                case RegularProfile regular:
                    profileType = ProfileType.Regular;
                    firstName = regular.FirstName;
                    lastName = regular.LastName;
                    cui = null!;
                    companyName = null!;
                    registrationNumber = null!;
                    address = null!;
                    county = null!;
                    locality = null!;
                    phoneNumbers = regular.PhoneNumbers;
                    break;
                case DealerProfile dealer:
                    profileType = ProfileType.Dealer;
                    firstName = null!;
                    lastName = null!;
                    cui = dealer.Cui;
                    companyName = dealer.CompanyName;
                    registrationNumber = dealer.RegistrationNumber;
                    address = dealer.Address;
                    county = dealer.County;
                    locality = dealer.Locality;
                    phoneNumbers = dealer.PhoneNumbers;
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

    [GeneratedRegex(@"^\S+@\S+\.\S+$")]
    private static partial Regex emailRegex();

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
        this.profileType = profileType;
        this.firstName = firstName;
        this.lastName = lastName;
        this.cui = cui;
        this.companyName = companyName;
        this.registrationNumber = registrationNumber;
        this.address = address;
        this.county = county;
        this.locality = locality;
        this.phoneNumbers = phoneNumbers;
    }

    private User(string email, string hashedPassword) :
        this(0, email, hashedPassword, false, DateTimeOffset.UtcNow,
            null, null, null, null, null, null, null, null, null, null)
    {
    }

    public static Result<User> Create(string email, string password)
    {
        if (!emailRegex().IsMatch(email))
            return Result.Failure<User>("Invalid email format");

        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < 8 ||
            password.Length > 32 ||
            !charactersRegex().IsMatch(password) ||
            !numbersRegex().IsMatch(password))
        {
            return Result.Failure<User>("Password must be between 8 and 32 characters long, and contain upper and lowercase characters and numbers");
        }

        var passwordhasher = new PasswordHasher<User>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        // In this context, passing null is acceptable because we are not using any user-specific data for hashing.
        var hashedPassword = passwordhasher.HashPassword(null, password);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        return new User(email.ToLowerInvariant(), hashedPassword);
    }

    internal bool ConfirmEmail(DateTime confirmedAt)
    {
        if (this.CreatedAt + TimeSpan.FromDays(7) < confirmedAt)
            return false;
        EmailConfirmed = true;
        return true;
    }
}

internal abstract record Profile(IReadOnlyList<string> PhoneNumbers);

internal sealed record RegularProfile : Profile
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    private RegularProfile(string firstName, string lastName, IEnumerable<string> phoneNumbers) : base(phoneNumbers.ToList())
    {
        FirstName = firstName;
        LastName = lastName;
    }

    internal static Result<RegularProfile> Create(string? firstName, string? lastName, IReadOnlyList<string>? phoneNumbers)
    {
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure<RegularProfile>("First name and last name must be provided");
        }

        if (phoneNumbers == null || phoneNumbers.Count == 0 || phoneNumbers.Any(p => string.IsNullOrWhiteSpace(p)))
        {
            return Result.Failure<RegularProfile>("At least one valid phone number must be provided");
        }

        return new RegularProfile(firstName, lastName, phoneNumbers);
    }
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
        string cui,
        string companyName,
        string registrationNumber,
        string address,
        string county,
        string locality,
        IEnumerable<string> phoneNumbers) : base(phoneNumbers.ToList())
    {
        Cui = cui;
        CompanyName = companyName;
        RegistrationNumber = registrationNumber;
        Address = address;
        County = county;
        Locality = locality;
    }

    internal static Result<DealerProfile> Create(
        string? cui,
        string? companyName,
        string? registrationNumber,
        string? address,
        string? county,
        string? locality,
        IEnumerable<string>? phoneNumbers)
    {
        if (string.IsNullOrWhiteSpace(cui) ||
            cui.Length != 8 ||
            !cui.All(char.IsDigit) ||
            string.IsNullOrWhiteSpace(companyName) ||
            string.IsNullOrWhiteSpace(registrationNumber) ||
            string.IsNullOrWhiteSpace(address) ||
            string.IsNullOrWhiteSpace(county) ||
            string.IsNullOrWhiteSpace(locality))
        {
            return Result.Failure<DealerProfile>("All company details must be provided for dealers");
        }

        if (phoneNumbers?.Any(p => string.IsNullOrWhiteSpace(p)) ?? false)
        {
            return Result.Failure<DealerProfile>("At least one valid phone number must be provided");
        }

        return new DealerProfile(cui, companyName, registrationNumber, address, county, locality, phoneNumbers ?? []);
    }
}

internal static class ProfileExtensions
{
    public static void Match(this Profile? profile, Action<RegularProfile> regularAction, Action<DealerProfile> dealerAction)
    {
        switch (profile)
        {
            case RegularProfile regular:
                regularAction(regular);
                break;
            case DealerProfile dealer:
                dealerAction(dealer);
                break;
            default:
                throw new InvalidOperationException("Unknown profile type");
        }
    }
}

internal enum ProfileType
{
    Regular,
    Dealer
}
