using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using UserIdentity.Emails;

namespace UserIdentity.Commands;

public interface IConfirmEmailCommand
{
    Task<Result> Execute(string token, CancellationToken cancellationToken);
}

internal sealed class ConfirmEmailCommand(
    IDataProtectionProvider dataProtectionProvider,
    UserIdentityDbContext dbContext) : IConfirmEmailCommand
{
    private const string Error = "Couldn't confirm email";

    public async Task<Result> Execute(
        string token, CancellationToken cancellationToken)
    {
        var emailConfirmationToken = new EmailConfirmationToken(
            dataProtectionProvider);
        var userResult = emailConfirmationToken.ParseToken(token);
        if (userResult.IsFailure)
            return Result.Failure(userResult.Error);

        var user = await dbContext.Users.FindAsync(
            [userResult.Value.Id], cancellationToken);
        
        if (user?.EmailConfirmed == true)
            return Result.Success();
        if (user?.Email != userResult.Value.Email)
            return Result.Failure(Error);

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
