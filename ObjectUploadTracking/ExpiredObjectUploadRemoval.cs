using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities.Base;

namespace ObjectUploadTracking;

internal sealed class ExpiredObjectUploadRemoval(
    ObjectUploadTrackingDbContext dbContext)
{
    private const int AmountToDelete = 1000;

    [TickerFunction("ExpiredObjectUploadRemoval", cronExpression: "0 * * * * *")]
    public async Task Execute(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();

        // Delete half an hour after expiry to ensure
        // that in progress uploads aren't deleted.
        var cutOffDate = DateTime.UtcNow.AddMinutes(-30);
        int deletedCount;
        do
        {
            deletedCount = await dbContext
                .ObjectUploads
                .Where(u => u.ExpiresAt < cutOffDate)
                .Take(AmountToDelete)
                .ExecuteDeleteAsync(cancellationToken);
        }
        while (deletedCount == AmountToDelete);
    }
}
