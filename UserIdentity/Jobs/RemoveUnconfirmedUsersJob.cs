using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities.Base;

namespace UserIdentity.Jobs;

internal sealed class RemoveUnconfirmedUsersJob(
    UserIdentityDbContext dbContext)
{
    private const int AmountToDelete = 1000;

    // Runs daily at 5 AM
    [TickerFunction("RemoveUnconfirmedUsers", cronExpression: "0 0 5 * * *")]
    public async Task Execute(
        TickerFunctionContext context, 
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();

        var cutOffDate = DateTime.UtcNow.AddDays(-7);
        int deletionCount;
        do
        {
            deletionCount = await dbContext
                .Users
                .Where(u => !u.EmailConfirmed && u.CreatedAt < cutOffDate)
                .Take(AmountToDelete)
                .ExecuteDeleteAsync(cancellationToken);
        }
        while (deletionCount == AmountToDelete);
    }
}
