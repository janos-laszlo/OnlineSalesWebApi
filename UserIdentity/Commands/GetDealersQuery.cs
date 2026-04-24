using Microsoft.EntityFrameworkCore;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface IGetDealersQuery
{
    Task<IReadOnlyList<UserInfoDto>> Execute(CancellationToken cancellationToken);
}

internal sealed class GetDealersQuery(UserIdentityDbContext dbContext) : IGetDealersQuery
{
    public async Task<IReadOnlyList<UserInfoDto>> Execute(CancellationToken cancellationToken) =>
        (await dbContext.Users
            .Where(u => u.ProfileType == ProfileType.Dealer)
            .ToListAsync(cancellationToken))
            .Select(UserInfoDto.From)
            .ToList();
}
