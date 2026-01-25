using UserIdentity.Entities;

namespace UserIdentity.Emails;

internal sealed class ConsoleEmailService : IEmailService
{
    public Task Send(Email email, CancellationToken cancellationToken)
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
