using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface ILoginUserCommand
{
    Result<TokenResponseDto> Execute(UserCredentialsDto userLoginDto);
}

internal sealed class LoginUserCommand(
    UserIdentityDbContext dbContext,
    JwtService jwtService) : ILoginUserCommand
{
    private const string Error = "Invalid email or password";
    private static readonly PasswordHasher<User> passwordHasher = new();

    public Result<TokenResponseDto> Execute(UserCredentialsDto userLoginDto)
    {
        var user = dbContext.Users.FirstOrDefault(
            u => u.Email == userLoginDto.Email);
        if (user == null)
            return Result.Failure<TokenResponseDto>(Error);

        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, userLoginDto.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return Result.Failure<TokenResponseDto>(Error);

        return jwtService.CreateTokens(user);
    }
}
