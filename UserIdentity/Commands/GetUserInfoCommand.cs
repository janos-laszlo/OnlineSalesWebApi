using CSharpFunctionalExtensions;

namespace UserIdentity.Commands;

public interface IGetUserInfoCommand
{
    Task<Result<UserInfoDto>> Execute(int userId, CancellationToken cancellationToken);
}

internal sealed class GetUserInfoCommand(UserIdentityDbContext dbContext) : IGetUserInfoCommand
{
    public async Task<Result<UserInfoDto>> Execute(int userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserInfoDto>("User not found");
        }

        var userInfo = new UserInfoDto(
            user.Id,
            user.Email,
            user.CreatedAt,
            user.EmailConfirmed);

        return userInfo;
    }
}

public sealed record UserInfoDto(
    int Id,
    string Email,
    DateTimeOffset CreatedAt,
    bool EmailConfirmed);
