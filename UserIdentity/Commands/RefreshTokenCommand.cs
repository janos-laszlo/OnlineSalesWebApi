using System.Text.Json;
using Common;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UserIdentity.Commands;

public interface IRefreshTokenCommand
{
    Task<Result<TokenResponseDto>> Execute(
        string refreshToken,
        CancellationToken cancellationToken);
}

internal sealed class RefreshTokenCommand(
    ILogger<RefreshTokenCommand> logger,
    IConfiguration configuration,
    UserIdentityDbContext dbContext,
    IDataProtectionProvider dataProtectionProvider,
    JwtService jwtService) : IRefreshTokenCommand
{
    private const string Error = "Invalid refresh token";

    public async Task<Result<TokenResponseDto>> Execute(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var protector = dataProtectionProvider.CreateProtector(
            configuration.GetValue<string>(Constants.ConfigKeys.JwtEncryptionKey)!);
        string refreshTokenString;
        try
        {
            refreshTokenString = protector.Unprotect(refreshToken);
        }
        catch
        {
            logger.LogWarning("Failed to unprotect refresh token {RefreshToken}", refreshToken);
            return Result.Failure<TokenResponseDto>(Error);
        }

        var refreshTokenObj = JsonSerializer.Deserialize<RefreshTokenDto>(refreshTokenString);
        if (refreshTokenObj is null)
        {
            logger.LogWarning("Failed to deserialize refresh token {RefreshToken}", refreshToken);
            return Result.Failure<TokenResponseDto>(Error);
        }

        if (DateTime.UtcNow > refreshTokenObj.ExpiresAt)
        {
            logger.LogWarning("Refresh token expired {RefreshToken}", refreshToken);
            return Result.Failure<TokenResponseDto>(Error);
        }

        var user = await dbContext
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == refreshTokenObj.UserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found for refresh token {RefreshToken}", refreshToken);
            return Result.Failure<TokenResponseDto>(Error);
        }

        return jwtService.CreateTokens(user);
    }
}
