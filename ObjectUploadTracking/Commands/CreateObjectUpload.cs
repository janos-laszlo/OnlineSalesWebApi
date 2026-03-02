namespace ObjectUploadTracking.Commands;

public interface ICreateObjectUpload
{
    Task Execute(
        ObjectUpload objectUpload,
        CancellationToken cancellation);
}

internal sealed class CreateObjectUpload(
    ObjectUploadTrackingDbContext dbContext) : ICreateObjectUpload
{
    public async Task Execute(
        ObjectUpload objectUpload,
        CancellationToken cancellation)
    {
        dbContext.ObjectUploads.Add(objectUpload);
        await dbContext.SaveChangesAsync(cancellation);
    }
}
