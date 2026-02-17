using Microsoft.Extensions.Logging;

namespace EmailSending;

internal abstract class EmailServiceBase(
    ILogger<EmailServiceBase> logger,
    EmailSendingDbContext dbContext) : IResilientEmailService
{
    protected abstract Task SendInternal(Email email, CancellationToken cancellationToken);

    async Task IResilientEmailService.Send(Email email, CancellationToken cancellationToken)
    {
        try
        {
            await SendInternal(email, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Queing email {EmailId}", email.Id);
            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
