using CSharpFunctionalExtensions;

namespace ObjectUploadTracking;

public interface IObjectUploadOperations
{
    Task Track(ObjectUpload objectUpload, CancellationToken cancellation);
    Task<Result> Consume(int objectUploadId, Func<ObjectUpload, Task<Result>> func, CancellationToken cancellation);
}

internal sealed class ObjectUploadOperations(
    ObjectUploadTrackingDbContext dbContext) : IObjectUploadOperations
{
    public async Task<Result> Consume(
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

    public async Task Track(ObjectUpload objectUpload, CancellationToken cancellation)
    {
        dbContext.ObjectUploads.Add(objectUpload);
        await dbContext.SaveChangesAsync(cancellation);
    }
}
