using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public sealed class RegisterUserCommand(ILogger<RegisterUserCommand> logger)
{
    public Result Execute(UserRegistrationDto userRegistrationDto)
    {
        var userResult = User.Create(
            userRegistrationDto.Email,
            userRegistrationDto.Password);
        if (userResult.IsFailure)
        {
            logger.LogError("Cannot register user because {Error}", userResult.Error);
            return userResult;
        }

        // save to database...
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("User with email {Email} registered successfully", userRegistrationDto.Email);
        
        return Result.Success();
    }

}

public sealed record UserRegistrationDto(string Email, string Password);
