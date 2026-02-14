using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities.Base;
using UserIdentity.Emails;

namespace UserIdentity.Jobs;

/// <summary>
/// Send any queued emails.
/// </summary>
internal sealed class SendEmailJob(
    UserIdentityDbContext dbContext,
    IEmailService emailService)
{
    public const string Name = "SendEmail";

    [TickerFunction(Name, "0 */5 * * * *")] // Every 5 minutes
    public async Task Execute(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();

        do
        {
            var emails = await dbContext
                .Emails
                .AsNoTracking()
                .Take(100)
                .ToArrayAsync(cancellationToken);

            if (emails.Length == 0)
                break;

            foreach (var email in emails)
                await emailService.Send(email, cancellationToken);

            IEnumerable<int> emailIds = emails.Select(em => em.Id).ToArray();
            await dbContext
                .Emails
                .Where(e => emailIds.Contains(e.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
        while (true);
    }
}
