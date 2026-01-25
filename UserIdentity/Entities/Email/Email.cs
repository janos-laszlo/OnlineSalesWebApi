using UserIdentity.Emails;

namespace UserIdentity.Entities;

internal sealed class Email
{
    public int Id { get; init; }

    public required string To { get; init; }

    public required string Subject { get; init; }

    public required string Body { get; init; }
}
