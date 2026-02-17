namespace EmailSending;

public interface INonResilientEmailService
{
    Task Send(Email email, CancellationToken cancellationToken);
}