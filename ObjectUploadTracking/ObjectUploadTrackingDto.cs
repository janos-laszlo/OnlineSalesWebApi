namespace ObjectUploadTracking;

public sealed record ObjectUploadTrackingDto(
    int Id,
    IDictionary<string, string> ObjectKeysAndTheirPresignedUploadUrls);
