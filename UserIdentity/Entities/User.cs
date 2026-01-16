using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace UserIdentity.Entities;

internal class User
{
    public long Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    private User(long id, string email, string passwordHash)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
    }

    public static Result<User> Create(string email, string passwordHash)
    {
        if (!Regex.IsMatch("^\\S+@\\S+\\.\\S+$", email))
            return Result.Failure<User>("Invalid email format");

        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length < 8)
            return Result.Failure<User>("Password hash must be at least 8 characters long");

        return new User(0, email, passwordHash);
    }
}
