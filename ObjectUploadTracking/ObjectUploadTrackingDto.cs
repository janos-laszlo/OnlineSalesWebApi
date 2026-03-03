namespace ObjectUploadTracking;

public sealed record ObjectUploadTrackingDto(
    int EntityId)
{
    public int? ObjectUploadId { get; init; }
    public IDictionary<string, string>? ObjectKeysAndTheirPresignedUploadUrls { get; init; }
}
