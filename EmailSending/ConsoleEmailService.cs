using Microsoft.Extensions.Logging;

namespace EmailSending;

internal sealed class ConsoleEmailService(
    ILogger<ConsoleEmailService> logger,
    EmailSendingDbContext dbContext) : EmailServiceBase(logger, dbContext), INonResilientEmailService
{
    async Task INonResilientEmailService.Send(Email email, CancellationToken cancellationToken) =>
        await SendInternal(email, cancellationToken);

    protected override Task SendInternal(Email email, CancellationToken cancellationToken)
    {
        Console.WriteLine("Sending Email:");
        Console.WriteLine($"To: {email.To}");
        Console.WriteLine($"Subject: {email.Subject}");
        Console.WriteLine("Body:");
        Console.WriteLine(email.Body);
        Console.WriteLine("Email sent successfully.");
        return Task.CompletedTask;
    }
}
