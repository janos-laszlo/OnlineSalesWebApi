using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface ICreateJWTCommand
{
    Result<string> Execute(string userEmail, string userPassword);
}

internal sealed class CreateJWTCommand(
    IConfiguration configuration,
    UserIdentityDbContext dbContext) : ICreateJWTCommand
{
    private const string Error = "Invalid email or password";

    public Result<string> Execute(string userEmail, string userPassword)
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Email == userEmail);
        if (user == null)
            return Result.Failure<string>(Error);
        var passwordHasher = new PasswordHasher<User>();
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userPassword);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return Result.Failure<string>(Error);

        return CreateToken(user);
    }

    private string CreateToken(User user)
    {
        var data = Encoding.UTF8.GetBytes(configuration.GetValue<string>("Jwt:EncryptionKey")!);
        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(data);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id,
            [JwtRegisteredClaimNames.Email] = user.Email
        };
        var descriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Issuer = "MyIssuer",
            Audience = "MyAudience",
            Claims = claims,
            IssuedAt = null,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(120),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }
}
