using CSharpFunctionalExtensions;

namespace UserIdentity.Commands;

public interface IGetUserProfileCommand
{
    Task<Result<UserInfoDto>> Execute(int userId, CancellationToken cancellationToken);
}

internal sealed class GetUserProfileCommand(UserIdentityDbContext dbContext) : IGetUserProfileCommand
{
    public async Task<Result<UserInfoDto>> Execute(
        int userId, CancellationToken cancellationToken)
    =>
        (await dbContext.Users
            .FindAsync([userId], cancellationToken))
            .AsMaybe()
            .ToResult("User not found")
            .Map(UserInfoDto.From);
}
