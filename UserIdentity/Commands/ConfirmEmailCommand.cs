using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using UserIdentity.Emails;

namespace UserIdentity.Commands;

public interface IConfirmEmailCommand
{
    Task<Result> Execute(string token, CancellationToken cancellationToken);
}

internal sealed class ConfirmEmailCommand(
    EmailConfirmationToken emailConfirmationToken,
    UserIdentityDbContext dbContext) : IConfirmEmailCommand
{
    private const string Error = "Couldn't confirm email";

    public async Task<Result> Execute(
        string token, CancellationToken cancellationToken)
    {
        var confirmationTokenPayload = emailConfirmationToken.ParseToken(token);
        if (confirmationTokenPayload.IsFailure)
            return Result.Failure(confirmationTokenPayload.Error);

        var user = await dbContext.Users.FindAsync(
            [confirmationTokenPayload.Value.Id], cancellationToken);
        
        if (user is null)
            return Result.Failure(Error);
        if (user.EmailConfirmed == true)
            return Result.Success();

        if (!user.ConfirmEmail(DateTime.UtcNow))
        {
            await dbContext.Users
                .Where(u => u.Id == user.Id)
                .ExecuteDeleteAsync(cancellationToken);
            return Result.Failure(Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
