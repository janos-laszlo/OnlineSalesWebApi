using UserIdentity.Entities;

namespace UserIdentity.Emails;

internal interface IEmailService
{
    Task Send(Email email, CancellationToken cancellationToken);
}
