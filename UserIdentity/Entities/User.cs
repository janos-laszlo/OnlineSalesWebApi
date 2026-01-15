using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;

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

    public static Fin<User> Create(string email, string password)
    {
        if(!Regex.IsMatch("^\\S+@\\S+\\.\\S+$", email))
            return Fin<User>.Fail(Error.New("Invalid email format"));

        var passwordHash = HashPassword(password);
        return new User(0, email, passwordHash);
    }

    private static string HashPassword(string password)
    {
        var hasher = new PassworHasher<User>();
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }
}
