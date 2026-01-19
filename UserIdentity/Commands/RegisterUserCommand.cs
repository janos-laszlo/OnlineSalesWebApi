using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface IRegisterUserCommand
{
    Result<int> Execute(UserCredentialsDto userRegistrationDto);
}

internal sealed class RegisterUserCommand(
    ILogger<RegisterUserCommand> logger,
    UserIdentityDbContext dbContext) : IRegisterUserCommand
{
    public Result<int> Execute(UserCredentialsDto userRegistrationDto)
    {
        var userResult = User.Create(
            userRegistrationDto.Email,
            userRegistrationDto.Password);
        if (userResult.IsFailure)
        {
            logger.LogError("Cannot register user because {Error}", userResult.Error);
            return Result.Failure<int>(userResult.Error);
        }

        if (dbContext.Users.Any(u => u.Email == userRegistrationDto.Email))
        {
            logger.LogError("User with email {UserRegistrationEmail} already exists", userRegistrationDto.Email);
            return Result.Failure<int>($"User with email {userRegistrationDto.Email} already exists");
        }

        dbContext.Users.Add(userResult.Value);
        dbContext.SaveChanges();
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("User with email {Email} registered successfully", userRegistrationDto.Email);

        return userResult.Value.Id;
    }

}
