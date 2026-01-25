using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models.Ticker;
using UserIdentity.Emails;
using UserIdentity.Entities;
using UserIdentity.Jobs;

namespace UserIdentity.Commands;

public interface IRegisterUserCommand
{
    Task<Result> Execute(
        UserCredentialsDto userRegistrationDto,
        CancellationToken cancellationToken);
}

internal sealed class RegisterUserCommand(
    ILogger<RegisterUserCommand> logger,
    UserIdentityDbContext dbContext,
    IDataProtectionProvider dataProtectionProvider,
    ITimeTickerManager<TimeTicker> timeTickerManager) : IRegisterUserCommand
{
    public async Task<Result> Execute(
        UserCredentialsDto userRegistrationDto,
        CancellationToken cancellationToken)
    {
        var userResult = User.Create(
            userRegistrationDto.Email,
            userRegistrationDto.Password);
        if (userResult.IsFailure)
        {
            logger.LogError("Cannot register user because {Error}", userResult.Error);
            return Result.Failure<int>(userResult.Error);
        }

        if (await dbContext.Users.AnyAsync(
            u => u.Email == userRegistrationDto.Email,
            cancellationToken))
        {
            logger.LogError("User with email {UserRegistrationEmail} already exists", userRegistrationDto.Email);
            return Result.Failure<int>($"User with email {userRegistrationDto.Email} already exists");
        }

        dbContext.Users.Add(userResult.Value);
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("User with email {Email} registered successfully", userRegistrationDto.Email);
        
        // Save user to generate Id
        await dbContext.SaveChangesAsync(cancellationToken);
        await CreateEmailConfirmationRequest(userResult.Value);

        return Result.Success();
    }

    private async Task CreateEmailConfirmationRequest(User user)
    {
        var emailConfirmationToken = new EmailConfirmationToken(
            dataProtectionProvider);
        var token = emailConfirmationToken.GenerateToken(
            user.Id, user.Email);
        var email = new Email(
            user.Email,
            "Please confirm your email address",
            $"""
                Dear user,
                Please confirm your email address by clicking the link below:
                http://localhost:5152/confirm-email?token={token}

                Thank you!
            """);
        await timeTickerManager.AddAsync(new TimeTicker
        {
            Function = SendEmailJob.SendEmail,
            Description = $"Request confirmation of {email.To} email address",
            Request = TickerHelper.CreateTickerRequest(email),
            ExecutionTime = DateTime.UtcNow.AddSeconds(10),
            Retries = 30,
            RetryIntervals = [5, 15, 30, 60, 120, 300, 600]
        });
    }
}
