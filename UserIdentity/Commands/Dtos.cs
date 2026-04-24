using CSharpFunctionalExtensions;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public sealed record UserCredentialsDto(string Email, string Password);
public sealed record TokenResponseDto(string AccessToken, string RefreshToken);
internal sealed record RefreshTokenDto(int UserId, DateTime ExpiresAt);
public sealed record RefreshTokenRequestDto(string RefreshToken);
public record UserProfileRequestDto(
    string? Email,
    string? FirstName,
    string? LastName,
    bool? IsDealer,
    string? Cui,
    string? CompanyName,
    string? RegistrationNumber,
    string? Address,
    string? County,
    string? Locality,
    IReadOnlyList<string?>? PhoneNumbers)
{
    internal Result<Profile> ToProfile() =>
        this.IsDealer is null
            ? NoProfile
                .Create(
                    this.Email,
                    this.PhoneNumbers)
                .Map(x => (Profile)x)
            : this.IsDealer == true
            ? DealerProfile
                .Create(
                    this.Email,
                    this.Cui,
                    this.CompanyName,
                    this.RegistrationNumber,
                    this.Address,
                    this.County,
                    this.Locality,
                    this.PhoneNumbers)
                .Map(x => (Profile)x)
            : RegularProfile
                .Create(
                    this.Email,
                    this.FirstName!,
                    this.LastName!,
                    this.PhoneNumbers)
                .Map(x => (Profile)x);
}

public sealed record UserInfoDto(string Email)
{
    public DateTimeOffset CreatedAt { get; init; }
    public bool EmailConfirmed { get; init; }
    public ProfileType? ProfileType { get; init; }
    public bool IsDealer { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Cui { get; init; }
    public string? CompanyName { get; init; }
    public string? RegistrationNumber { get; init; }
    public string? Address { get; init; }
    public string? County { get; init; }
    public string? Locality { get; init; }
    public IReadOnlyList<string>? PhoneNumbers { get; init; }

    internal static UserInfoDto From(User user) =>
        user.Profile.Match(
            regular => new UserInfoDto(regular.Email)
            {
                CreatedAt = user.CreatedAt,
                EmailConfirmed = user.EmailConfirmed,
                ProfileType = Entities.ProfileType.Regular,
                IsDealer = false,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumbers = regular.PhoneNumbers
            },
            dealer => new UserInfoDto(dealer.Email)
            {
                CreatedAt = user.CreatedAt,
                EmailConfirmed = user.EmailConfirmed,
                ProfileType = Entities.ProfileType.Dealer,
                IsDealer = true,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Cui = dealer.Cui,
                CompanyName = dealer.CompanyName,
                RegistrationNumber = dealer.RegistrationNumber,
                Address = dealer.Address,
                County = dealer.County,
                Locality = dealer.Locality,
                PhoneNumbers = dealer.PhoneNumbers
            },
            noProfileAction => new UserInfoDto(noProfileAction.Email)
            {
                CreatedAt = user.CreatedAt,
                EmailConfirmed = user.EmailConfirmed,
                ProfileType = null,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumbers = noProfileAction.PhoneNumbers
            },
            admin => new UserInfoDto(admin.Email)
            {
                CreatedAt = user.CreatedAt,
                EmailConfirmed = user.EmailConfirmed,
                ProfileType = Entities.ProfileType.Admin,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumbers = admin.PhoneNumbers
            });
}

internal sealed record EmailConfirmationTokenPayload(int Id, string Email);
