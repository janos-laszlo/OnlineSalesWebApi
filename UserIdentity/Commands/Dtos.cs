namespace UserIdentity.Commands;

public sealed record UserCredentialsDto(string Email, string Password);
public sealed record TokenResponseDto(string AccessToken, string RefreshToken);
public sealed record RefreshTokenDto(int UserId, DateTime ExpiresAt);
public sealed record RefreshTokenRequestDto(string RefreshToken);