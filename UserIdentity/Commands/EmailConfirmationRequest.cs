using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserIdentity.Emails;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

internal sealed class EmailConfirmationRequest(
    IEmailService emailService,
    ILogger<EmailConfirmationRequest> logger,
    UserIdentityDbContext dbContext,
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
            Body = $"""
                Dear user,
                Please confirm your email address by clicking the link below:
                {baseUrl}/confirm-email?token={token}

                Thank you!
            """
        };
        
        try
        {
            await emailService.Send(email, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email confirmation request to {Email}", user.Email);
            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
