using CSharpFunctionalExtensions;

namespace UserIdentity.Commands;

public interface IUpdateUserProfileCommand
{
    Task<Result> Execute(int userId, UserProfileRequestDto userProfile, CancellationToken cancellationToken);
}

internal sealed class UpdateUserProfileCommand(
    UserIdentityDbContext dbContext) : IUpdateUserProfileCommand
{
    public async Task<Result> Execute(int userId, UserProfileRequestDto userProfile, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([userId], cancellationToken);
        if (user == null)
            return Result.Failure("User not found");

        var result = userProfile.ToProfile();
        if (result.IsFailure)
            return Result.Failure(result.Error);
        user.Profile = result.Value;
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
