using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface ILoginUserCommand
{
    Result<string> Execute(UserCredentialsDto userLoginDto);
}

internal sealed class LoginUserCommand(
    IConfiguration configuration,
    UserIdentityDbContext dbContext) : ILoginUserCommand
{
    private const string Error = "Invalid email or password";
    private static readonly JsonWebTokenHandler handler = new();
    private static readonly PasswordHasher<User> passwordHasher = new();

    public Result<string> Execute(UserCredentialsDto userLoginDto)
    {
        var user = dbContext.Users.FirstOrDefault(
            u => u.Email == userLoginDto.Email);
        if (user == null)
            return Result.Failure<string>(Error);

        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, userLoginDto.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return Result.Failure<string>(Error);

        return CreateToken(user);
    }

    private string CreateToken(User user)
    {
        var data = Encoding.UTF8.GetBytes(
            configuration.GetValue<string>("Jwt:EncryptionKey")!);
        var securityKey = new SymmetricSecurityKey(data);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id,
            [JwtRegisteredClaimNames.Email] = user.Email
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "http://localhost:5152",
            Audience = "http://localhost:5152",
            Claims = claims,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(120),
            SigningCredentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256)
        };

        return handler.CreateToken(descriptor);
    }
}
