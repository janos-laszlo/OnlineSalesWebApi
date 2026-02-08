namespace UserIdentity.Commands;

public sealed record UserCredentialsDto(string Email, string Password);
public sealed record TokenResponseDto(string AccessToken, string RefreshToken);
public sealed record RefreshTokenDto(int UserId, DateTime ExpiresAt);
public sealed record RefreshTokenRequestDto(string RefreshToken);
public record UserProfileRequestDto(
    string? Email,
    string? FirstName,
    string? LastName,
    bool IsDealer,
    string? Cui,
    string? CompanyName,
    string? RegistrationNumber,
    string? Address,
    string? County,
    string? Locality,
    IReadOnlyList<string>? PhoneNumbers);
    
public sealed record UserInfoDto(
    int Id,
    string Email,
    DateTimeOffset CreatedAt,
    bool EmailConfirmed,
    bool IsDealer,
    string? FirstName,
    string? LastName,
    string? Cui,
    string? CompanyName,
    string? RegistrationNumber,
    string? Address,
    string? County,
    string? Locality,
    IReadOnlyList<string>? PhoneNumbers);

internal sealed record EmailConfirmationTokenPayload(int Id, string Email);