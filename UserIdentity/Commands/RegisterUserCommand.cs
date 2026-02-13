using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    EmailConfirmationRequest emailConfirmationRequest) : IRegisterUserCommand
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
        await emailConfirmationRequest.Send(userResult.Value, cancellationToken);

        return Result.Success();
    }
}
