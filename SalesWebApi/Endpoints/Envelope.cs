namespace SalesWebApi.Endpoints;

public sealed record Envelope
{
    public object? Data { get; }
    public string? Message { get; }

    private Envelope(object? data, string? message)
    {
        Data = data;
        Message = message;
    }

    public static Envelope Success(object data) => new(data, null);
    public static Envelope Failure(string message) => new(null, message);
};