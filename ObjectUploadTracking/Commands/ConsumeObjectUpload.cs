using CSharpFunctionalExtensions;

namespace ObjectUploadTracking.Commands;

public interface IConsumeObjectUpload
{
    Task<Result> Execute(
        int objectUploadId,
        Func<ObjectUpload, Task<Result>> func,
        CancellationToken cancellation);
}

internal sealed class ConsumeObjectUpload(
    ObjectUploadTrackingDbContext dbContext) : IConsumeObjectUpload
{
    public async Task<Result> Execute(
        int objectUploadId,
        Func<ObjectUpload, Task<Result>> func,
        CancellationToken cancellation)
    {
        var objectUpload = dbContext.ObjectUploads.Find(objectUploadId);
        if (objectUpload is null)
            return Result.Failure($"Object upload with id {objectUploadId} not found.");

        var result = await func(objectUpload);
        dbContext.ObjectUploads.Remove(objectUpload);
        await dbContext.SaveChangesAsync(cancellation);
        return result;
    }
}
