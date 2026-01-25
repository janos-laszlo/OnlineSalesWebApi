namespace UserIdentity.Emails;

internal sealed record Email(
    string To,
    string Subject,
    string Body);
