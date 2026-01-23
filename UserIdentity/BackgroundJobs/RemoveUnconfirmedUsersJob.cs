using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities.Base;

namespace UserIdentity.BackgroundJobs;

internal sealed class RemoveUnconfirmedUsersJob(
    UserIdentityDbContext dbContext)
{
    // Runs daily at 5 AM
    [TickerFunction("RemoveUnconfirmedUsers", cronExpression: "0 0 5 * * *")]
    public async Task Execute(CancellationToken cancellationToken)
    {
        var cutOffDate = DateTime.UtcNow.AddDays(-7);
        int deletionCount;
        do
        {
            deletionCount = await dbContext
                .Users
                .Where(u => !u.EmailConfirmed && u.CreatedAt < cutOffDate)
                .Take(1000)
                .ExecuteDeleteAsync(cancellationToken);
        }
        while (deletionCount > 0);
    }
}
