using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using UserIdentity.Commands;
using UserIdentity.Entities;

namespace UserIdentity.Emails;

internal sealed class EmailConfirmationToken(
    IDataProtectionProvider dataProtectionProvider)
{
    private const string Error = "Invalid email confirmation token";
    private readonly IDataProtector protector =
        dataProtectionProvider.CreateProtector("EmailConfirmationToken");

    public string GenerateToken(int id, string email) =>
        Uri.EscapeDataString(
            this.protector.Protect(
                JsonSerializer.Serialize(
                    new EmailConfirmationTokenPayload(
                        id,email))));

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
