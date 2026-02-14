using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserIdentity.Emails;

namespace UserIdentity.Commands;

public interface IConfirmEmailCommand
{
    Task<Result> Execute(string token, CancellationToken cancellationToken);
}

internal sealed class ConfirmEmailCommand(
    ILogger<ConfirmEmailCommand> logger,
    EmailConfirmationToken emailConfirmationToken,
    UserIdentityDbContext dbContext) : IConfirmEmailCommand
{
    private const string Error = "Couldn't confirm email";

    public async Task<Result> Execute(
        string token, CancellationToken cancellationToken)
    {
        var confirmationTokenPayload = emailConfirmationToken.ParseToken(token);
        if (confirmationTokenPayload.IsFailure)
        {
            logger.LogWarning(
                "Failed to parse email confirmation token: {ConfirmationToken}, Error: {Error}",
                token,
                confirmationTokenPayload.Error);
            return Result.Failure(confirmationTokenPayload.Error);
        }

        var user = await dbContext.Users.FindAsync(
            [confirmationTokenPayload.Value.Id], cancellationToken);

        if (user is null)
        {
            logger.LogWarning(
                "User with id {UserId} not found for email confirmation",
                confirmationTokenPayload.Value.Id);
            return Result.Failure(Error);
        }

        if (user.EmailConfirmed)
            return Result.Success();

        if (!user.ConfirmEmail(DateTime.UtcNow))
        {
            logger.LogWarning(
                "Failed to confirm email for user with id {UserId}",
                user.Id);
            await dbContext.Users
                .Where(u => u.Id == user.Id)
                .ExecuteDeleteAsync(cancellationToken);
            return Result.Failure(Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
