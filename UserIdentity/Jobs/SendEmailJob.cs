using TickerQ.Utilities.Base;
using TickerQ.Utilities.Models;
using UserIdentity.Emails;

namespace UserIdentity.Jobs;

internal sealed class SendEmailJob(
    IEmailService emailService)
{
    public const string SendEmail = "SendEmail";

    [TickerFunction(SendEmail)]
    public async Task Execute(
        TickerFunctionContext<Email> context,
        CancellationToken cancellationToken)
    {
        var email = context.Request;
        await emailService.Send(email, cancellationToken);
    }
}
