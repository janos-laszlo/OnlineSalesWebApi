using CSharpFunctionalExtensions;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface IGetUserInfoCommand
{
    Task<Result<UserInfoDto>> Execute(int userId, CancellationToken cancellationToken);
}

internal sealed class GetUserInfoCommand(UserIdentityDbContext dbContext) : IGetUserInfoCommand
{
    public async Task<Result<UserInfoDto>> Execute(int userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserInfoDto>("User not found");
        }

        return UserInfoDto.From(user);
    }
}
