using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using UserIdentity.Commands;

namespace UserIdentity.Emails;

internal sealed class EmailConfirmationToken(
    IDataProtectionProvider dataProtectionProvider,
    IConfiguration configuration)
{
    private const string Error = "Invalid email confirmation token";
    internal const string PurposeKey = "DataProtection:EmailConfirmationTokenPurpose";
    private readonly IDataProtector protector =
        dataProtectionProvider.CreateProtector(
            configuration[PurposeKey] ??
            throw new Exception($"{PurposeKey} configuration is missing"));

    public string GenerateToken(int id, string email) =>
        Uri.EscapeDataString(
            this.protector.Protect(
                JsonSerializer.Serialize(
                    new EmailConfirmationTokenPayload(
                        id, email))));

    public Result<EmailConfirmationTokenPayload> ParseToken(string token)
    {
        try
        {
            var user = JsonSerializer.Deserialize<EmailConfirmationTokenPayload>(
                this.protector.Unprotect(Uri.UnescapeDataString(token)));
            if (user is null)
                return Result.Failure<EmailConfirmationTokenPayload>(Error);
            return user;
        }
        catch (Exception)
        {
            return Result.Failure<EmailConfirmationTokenPayload>(Error);
        }
    }
}
