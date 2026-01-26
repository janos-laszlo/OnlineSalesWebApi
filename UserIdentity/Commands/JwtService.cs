using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

internal sealed class JwtService(
    IConfiguration configuration,
    IDataProtectionProvider dataProtectionProvider)
{
    private const string EncryptionKeyConfigKey = "Jwt:EncryptionKey";
    private readonly string baseUrl = configuration["BaseUrl"] ??
        throw new Exception("BaseUrl configuration is missing");
    private static readonly JsonWebTokenHandler handler = new();

    public TokenResponseDto CreateTokens(User user) =>
        new(CreateToken(user),
            CreateRefreshToken(user));

    private string CreateToken(User user)
    {
        var data = Encoding.UTF8.GetBytes(
            configuration.GetValue<string>(EncryptionKeyConfigKey)!);
        var securityKey = new SymmetricSecurityKey(data);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id,
            [JwtRegisteredClaimNames.Email] = user.Email
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = baseUrl,
            Audience = baseUrl,
            Claims = claims,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(120),
            SigningCredentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256)
        };

        return handler.CreateToken(descriptor);
    }

    private string CreateRefreshToken(User user)
    {
        var refreshTokenObj = new RefreshTokenDto(user.Id, DateTime.UtcNow.AddMonths(1));
        var refreshTokenString = JsonSerializer.Serialize(refreshTokenObj);
        var protector = dataProtectionProvider.CreateProtector(
            configuration.GetValue<string>(EncryptionKeyConfigKey)!);
        return protector.Protect(refreshTokenString);
    }
}
