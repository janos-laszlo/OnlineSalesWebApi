using CSharpFunctionalExtensions;

namespace ObjectUploadTracking.Commands;

public interface IConsumeObjectUpload
{
    Task<UnitResult<ObjectUploadErrorCode>> Execute(
        int objectUploadId,
        Func<ObjectUpload, Task<UnitResult<ObjectUploadErrorCode>>> func,
        CancellationToken cancellation);
}

public enum ObjectUploadErrorCode
{
    ObjectUploadNotFound,
    EntityNotFound,
    ObjectUploadDoesNotBelongToUser,
}

internal sealed class ConsumeObjectUpload(
    ObjectUploadTrackingDbContext dbContext) : IConsumeObjectUpload
{
    public async Task<UnitResult<ObjectUploadErrorCode>> Execute(
        int objectUploadId,
        Func<ObjectUpload, Task<UnitResult<ObjectUploadErrorCode>>> func,
        CancellationToken cancellation)
    {
        var objectUpload = dbContext.ObjectUploads.Find(objectUploadId);
        if (objectUpload is null)
            return ObjectUploadErrorCode.ObjectUploadNotFound;

        var result = await func(objectUpload);
        dbContext.ObjectUploads.Remove(objectUpload);
        await dbContext.SaveChangesAsync(cancellation);
        return result;
    }
}
