using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;

namespace UserIdentity.Entities;

internal partial class User
{
    public long Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    [GeneratedRegex("(?=.*[a-z])(?=.*[A-Z])")]
    private static partial Regex charactersRegex();
    
    [GeneratedRegex("[0-9]")]
    private static partial Regex numbersRegex();
    
    [GeneratedRegex(@"^\S+@\S+\.\S+$")]
    private static partial Regex emailRegex();

    private User(long id, string email, string passwordHash)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
    }

    public static Result<User> Create(string email, string password)
    {
        if (!emailRegex().IsMatch(email))
            return Result.Failure<User>("Invalid email format");

        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < 8 ||
            password.Length > 32 ||
            !charactersRegex().IsMatch(password) ||
            !numbersRegex().IsMatch(password))
        {
            return Result.Failure<User>("Password must be between 8 and 32 characters long, and contain upper and lowercase characters and numbers");
        }

        var passwordhasher = new PasswordHasher<User>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        // In this context, passing null is acceptable because we are not using any user-specific data for hashing.
        var hashedPassword = passwordhasher.HashPassword(null, password);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        return new User(0, email, hashedPassword);
    }
}
