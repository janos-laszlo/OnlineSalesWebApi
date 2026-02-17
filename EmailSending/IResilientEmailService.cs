namespace EmailSending;

public interface IResilientEmailService
{
    Task Send(Email email, CancellationToken cancellationToken);
}
