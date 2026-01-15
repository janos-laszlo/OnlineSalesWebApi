namespace UserIdentity.Commands;

public class RegisterUserCommand
{
    public void Execute(UserRegistrationDto userRegistrationDto)
    {
        // var user = User.Create(userRegistrationDto.Email, userRegistrationDto.Password);
    }

    public sealed record UserRegistrationDto(string Email, string Password);
}
