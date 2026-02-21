using EmailSending;
using Microsoft.Extensions.Configuration;
using UserIdentity.Emails;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

internal sealed class EmailConfirmationRequest(
    IResilientEmailService emailService,
    IConfiguration configuration,
    EmailConfirmationToken emailConfirmationToken)
{
    private readonly string baseUrl = configuration["BaseUrl"] ??
        throw new Exception("BaseUrl configuration is missing");

    internal async Task Send(
        User user, CancellationToken cancellationToken)
    {
        var token = emailConfirmationToken.GenerateToken(
            user.Id, user.Email);
        var email = new Email
        {
            To = user.Email,
            Subject = "Please confirm your email address",
            // TODO: Use a proper email template and use button instead of link
            Body = $"""
                Dear user,
                Please confirm your email address by accessing the link below:
                {baseUrl}/confirm-email?token={token}

                Thank you!
            """
        };

        await emailService.Send(email, cancellationToken);
    }
}
