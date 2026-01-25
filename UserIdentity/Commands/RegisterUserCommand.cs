using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserIdentity.Emails;
using UserIdentity.Entities;

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
    IDataProtectionProvider dataProtectionProvider) : IRegisterUserCommand
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
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task CreateEmailConfirmationRequest(User user)
    {
        var emailConfirmationToken = new EmailConfirmationToken(
            dataProtectionProvider);
        var token = emailConfirmationToken.GenerateToken(
            user.Id, user.Email);
        var email = new Email
        {
            To = user.Email,
            Subject = "Please confirm your email address",
            Body = 
                $"""
                Dear user,
                Please confirm your email address by clicking the link below:
                http://localhost:5152/confirm-email?token={token}

                Thank you!
                """
        };
        dbContext.Emails.Add(email);
    }
}
