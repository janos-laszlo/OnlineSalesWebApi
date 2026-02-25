using EmailSending;
using System.Collections.ObjectModel;

namespace SalesWebApi.IntegrationTests;

internal sealed class EmailServiceStub : IResilientEmailService
{
    private readonly Collection<Email> emails = [];
    internal IReadOnlyCollection<Email> Emails => emails;
    public Task Send(Email email, CancellationToken cancellationToken)
    {
        emails.Add(email);
        return Task.CompletedTask;
    }
}
