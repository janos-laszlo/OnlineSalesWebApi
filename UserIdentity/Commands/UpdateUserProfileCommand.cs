using CSharpFunctionalExtensions;

namespace UserIdentity.Commands;

public interface IUpdateUserProfileCommand
{
    Task<Result> Execute(
        int userId,
        UserProfileRequestDto userProfile,
        CancellationToken cancellationToken);
}

internal sealed class UpdateUserProfileCommand(
    UserIdentityDbContext dbContext,
    EmailConfirmationRequest emailConfirmationRequest) : IUpdateUserProfileCommand
{
    public async Task<Result> Execute(
        int userId,
        UserProfileRequestDto userProfile,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([userId], cancellationToken);
        if (user == null)
            return Result.Failure("User not found");

        var result = userProfile.ToProfile();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        var previousEmail = user.Email;
        user.Profile = result.Value;
        await dbContext.SaveChangesAsync(cancellationToken);
        if (previousEmail != user.Email)
            await emailConfirmationRequest.Send(user, cancellationToken);

        return result;
    }
}
