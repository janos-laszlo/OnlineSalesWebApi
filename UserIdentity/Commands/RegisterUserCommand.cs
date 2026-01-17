using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface IRegisterUserCommand
{
    Result Execute(UserRegistrationDto userRegistrationDto);
}

internal sealed class RegisterUserCommand(
    ILogger<RegisterUserCommand> logger,
    UserIdentityDbContext dbContext) : IRegisterUserCommand
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
        // TODO: Check for existing user with the same email
        // TODO: Add a CreatedAt timestamp to remove users who never confirm their email
        dbContext.Users.Add(userResult.Value);
        dbContext.SaveChanges();
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("User with email {Email} registered successfully", userRegistrationDto.Email);
        
        return Result.Success();
    }

}

public sealed record UserRegistrationDto(string Email, string Password);
