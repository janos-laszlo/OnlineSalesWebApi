using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface ILoginUserCommand
{
    Task<Result<TokenResponseDto>> Execute(
        UserCredentialsDto userLoginDto,
        CancellationToken cancellationToken);
}

internal sealed class LoginUserCommand(
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
        if (user == null) // TODO: || !user.EmailConfirmed
            return Result.Failure<TokenResponseDto>(Error);

        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, userLoginDto.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return Result.Failure<TokenResponseDto>(Error);

        return jwtService.CreateTokens(user);
    }
}
