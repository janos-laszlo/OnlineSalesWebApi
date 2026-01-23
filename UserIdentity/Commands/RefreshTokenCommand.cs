using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UserIdentity.Commands;

public interface IRefreshTokenCommand
{
    Result<TokenResponseDto> Execute(string refreshToken);
}

internal sealed class RefreshTokenCommand(
    IConfiguration configuration,
    UserIdentityDbContext dbContext,
    IDataProtectionProvider dataProtectionProvider,
    JwtService jwtService) : IRefreshTokenCommand
{
    private const string EncryptionKeyConfigKey = "Jwt:EncryptionKey";
    private const string Error = "Invalid refresh token";

    public Result<TokenResponseDto> Execute(string refreshToken)
    {
        var protector = dataProtectionProvider.CreateProtector(
            configuration.GetValue<string>(EncryptionKeyConfigKey)!);
        string refreshTokenString;
        try
        {
            refreshTokenString = protector.Unprotect(refreshToken);
        }
        catch
        {
            return Result.Failure<TokenResponseDto>(Error);
        }

        var refreshTokenObj = JsonSerializer.Deserialize<RefreshTokenDto>(refreshTokenString);
        if (refreshTokenObj is null)
        {
            return Result.Failure<TokenResponseDto>(Error);
        }

        if (DateTime.UtcNow > refreshTokenObj.ExpiresAt)
        {
            return Result.Failure<TokenResponseDto>(Error);
        }

        var user = dbContext
            .Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == refreshTokenObj.UserId);
        if (user == null)
        {
            return Result.Failure<TokenResponseDto>(Error);
        }

        return jwtService.CreateTokens(user);
    }
}
