using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;

namespace ObjectUploadTracking;

public interface IObjectUploadOperations
{
    Task<ObjectUploadTrackingDto> CreateObjectUpload(
        string bucketName,
        IEnumerable<string> objectContentTypes,
        int entityId,
        DateTime expiresAt,
        CancellationToken cancellation);

    Task<Result> Consume(int objectUploadId, Func<ObjectUpload, Task<Result>> func, CancellationToken cancellation);
}

internal sealed class ObjectUploadOperations(
    ObjectUploadTrackingDbContext dbContext,
    IAmazonS3 r2Client) : IObjectUploadOperations
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

    public async Task<ObjectUploadTrackingDto> CreateObjectUpload(
        string bucketName,
        IEnumerable<string> objectContentTypes,
        int entityId,
        DateTime expiresAt,
        CancellationToken cancellation)
    {
        IReadOnlyList<(ObjectKeyName, string)> objKeyAndItsContentType = CreateObjectKeys(objectContentTypes);
        ObjectUpload objectUpload = new()
        {
            EntityId = entityId,
            Directory = DirectoryName.Create(Guid.CreateVersion7().ToString("N")).Value,
            ObjectKeys = [.. objKeyAndItsContentType.Select(tuple => tuple.Item1)],
            ExpiresAt = expiresAt
        };
        dbContext.ObjectUploads.Add(objectUpload);
        await dbContext.SaveChangesAsync(cancellation);

        return new ObjectUploadTrackingDto(
            objectUpload.Id,
            CreatePresignedPutRequests(bucketName, objectUpload.Directory, objKeyAndItsContentType));
    }

    private static List<(ObjectKeyName, string)> CreateObjectKeys(
        IEnumerable<string> objectContentTypes)
    =>
        [.. objectContentTypes
            .Select((pct, index) => (ObjectKeyName.Create($"{index}.{pct.Split('/')[1]}").Value, pct))];

    private Dictionary<string, string> CreatePresignedPutRequests(
        string bucketName,
        DirectoryName directory,
        IReadOnlyList<(ObjectKeyName, string)> objectKeysAndTheirContentType)
    =>
        objectKeysAndTheirContentType.ToDictionary(
            objectKey => objectKey.Item1.Value,
            objectKeyAndItsContentType => r2Client.GetPreSignedURL(
                new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = $"{directory.Value}/{objectKeyAndItsContentType.Item1.Value}",
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.Now.AddMinutes(15),
                    ContentType = objectKeyAndItsContentType.Item2
                }));


}

public sealed record ObjectUploadTrackingDto(
    int Id,
    IDictionary<string, string> ObjectKeysAndTheirPresignedUploadUrls);
