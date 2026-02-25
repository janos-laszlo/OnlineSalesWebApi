using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface ILoginUserCommand
{
    Task<Result<TokenResponseDto>> Execute(
        UserCredentialsDto userLoginDto,
        CancellationToken cancellationToken);
}

internal sealed class LoginUserCommand(
    ILogger<LoginUserCommand> logger,
    UserIdentityDbContext dbContext,
    JwtService jwtService) : ILoginUserCommand
{
    private const string Error = "Invalid email or password";
    private static readonly PasswordHasher<User> passwordHasher = new();

    public async Task<Result<TokenResponseDto>> Execute(
        UserCredentialsDto userLoginDto,
        CancellationToken cancellationToken)
    {
        var user = await dbContext
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == userLoginDto.Email, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("User not found for email: {Email}", userLoginDto.Email);
            return Result.Failure<TokenResponseDto>(Error);
        }
#if !DEBUG
        if (!user.EmailConfirmed)
        {
            logger.LogWarning("User not found for email: {Email}", userLoginDto.Email);
            return Result.Failure<TokenResponseDto>(Error);
        }
#endif
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, userLoginDto.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Invalid password for email: {Email}", userLoginDto.Email);
            return Result.Failure<TokenResponseDto>(Error);
        }

        return jwtService.CreateTokens(user);
    }
}
