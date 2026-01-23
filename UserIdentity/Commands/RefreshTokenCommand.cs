using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UserIdentity.Commands;

public interface IRefreshTokenCommand
{
    Task<Result<TokenResponseDto>> Execute(
        string refreshToken,
        CancellationToken cancellationToken);
}

internal sealed class RefreshTokenCommand(
    IConfiguration configuration,
    UserIdentityDbContext dbContext,
    IDataProtectionProvider dataProtectionProvider,
    JwtService jwtService) : IRefreshTokenCommand
{
    private const string EncryptionKeyConfigKey = "Jwt:EncryptionKey";
    private const string Error = "Invalid refresh token";

    public async Task<Result<TokenResponseDto>> Execute(
        string refreshToken,
        CancellationToken cancellationToken)
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

        var user = await dbContext
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == refreshTokenObj.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<TokenResponseDto>(Error);
        }

        return jwtService.CreateTokens(user);
    }
}
