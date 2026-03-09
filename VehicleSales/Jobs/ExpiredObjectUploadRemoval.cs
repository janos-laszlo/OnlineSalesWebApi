using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using ObjectUploadTracking;
using ObjectUploadTracking.Commands;
using TickerQ.Utilities.Base;

namespace VehicleSales.Jobs;

// TODO: Move this job to VehicleSales. Add a Module column to ObjectUpload.
// When removing expired object uploads remove the uploaded but unconfirmed objects as well.
internal sealed class ExpiredObjectUploadRemoval(
    IConsumeExpiredObjectUploads consumeExpiredObjectUploads,
    IConfiguration configuration,
    IAmazonS3 r2Client)
{
    // Every 5 minutes.
    [TickerFunction("ExpiredObjectUploadRemoval", cronExpression: "0 */5 * * * *")]
    public async Task Execute(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();

        // Delete 5 minutes after expiry to ensure
        // that in progress uploads aren't deleted.
        var cutOffDateTime = DateTime.UtcNow.AddMinutes(-5);

        await consumeExpiredObjectUploads.Execute(
            Constants.ModuleName,
            cutOffDateTime,
            o => RemoveUploadedObjects(o, cancellationToken),
            cancellationToken);
    }

    private async Task RemoveUploadedObjects(ObjectUpload expiredObjectUpload, CancellationToken cancellation)
    {
        var bucketName = configuration[R2Config.BucketNameKey] ??
            throw new InvalidOperationException("Bucket name not set in configuration");
        var request = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = expiredObjectUpload.Directory.Value
        };

        var response = await r2Client.ListObjectsV2Async(request, cancellation);

        List<KeyVersion> objectsToRemove = [.. response.S3Objects
                ?.Where(o => expiredObjectUpload.ObjectKeys.Any(ok => o.Key.EndsWith(ok.Value)))
                .Select(o => new KeyVersion { Key = o.Key }) ?? []];
        if (objectsToRemove.Count == 0)
            return;

        var deleteRequest = new DeleteObjectsRequest
        {
            BucketName = bucketName,
            Objects = objectsToRemove
        };

        await r2Client.DeleteObjectsAsync(deleteRequest, cancellation);
    }
}
