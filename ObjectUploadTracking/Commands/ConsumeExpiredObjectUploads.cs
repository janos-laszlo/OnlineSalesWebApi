using Microsoft.EntityFrameworkCore;

namespace ObjectUploadTracking.Commands;

public interface IConsumeExpiredObjectUploads
{
    Task Execute(
        string module,
        DateTime expiration,
        Func<ObjectUpload, Task> func,
        CancellationToken cancellationToken);
}

internal sealed class ConsumeExpiredObjectUploads(
    ObjectUploadTrackingDbContext dbContext) : IConsumeExpiredObjectUploads
{
    private const int BatchSize = 100;

    public async Task Execute(
        string module,
        DateTime expiration,
        Func<ObjectUpload, Task> func,
        CancellationToken cancellationToken)
    {
        List<ObjectUpload> expiredObjectUploads;
        do
        {
            expiredObjectUploads = await dbContext.ObjectUploads
                .Where(o => o.Module == module && o.ExpiresAt <= expiration)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
            if (expiredObjectUploads.Count == 0)
                break;

            foreach (var objectUpload in expiredObjectUploads)
                await func(objectUpload);

            dbContext.ObjectUploads.RemoveRange(expiredObjectUploads);
            await dbContext.SaveChangesAsync(cancellationToken);
        } while (expiredObjectUploads.Count == BatchSize);
    }
}
