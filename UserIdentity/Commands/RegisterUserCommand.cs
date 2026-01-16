using UserIdentity.Entities;

namespace UserIdentity.Commands;

public class RegisterUserCommand
{
    public void Execute(UserRegistrationDto userRegistrationDto)
    {
        var userResult = User.Create(userRegistrationDto.Email, userRegistrationDto.Password);
        if (userResult.IsFailure)
        {
            Console.WriteLine(userResult.Error);
            return;
        }
    }

    public sealed record UserRegistrationDto(string Email, string Password);
}
