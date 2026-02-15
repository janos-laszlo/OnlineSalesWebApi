using System.Collections.ObjectModel;
using UserIdentity.Emails;
using UserIdentity.Entities;

namespace SalesWebApi.IntegrationTests;

internal sealed class EmailServiceStub : IEmailService
{
    private readonly Collection<Email> emails = [];
    internal IReadOnlyCollection<Email> Emails => emails;
    public Task Send(Email email, CancellationToken cancellationToken)
    {
        emails.Add(email);
        return Task.CompletedTask;
    }
}
