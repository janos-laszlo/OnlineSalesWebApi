namespace EmailSending;

public sealed class Email
{
    public int Id { get; private set; }

    public required string To { get; init; }

    public required string Subject { get; init; }

    public required string Body { get; init; }
}
