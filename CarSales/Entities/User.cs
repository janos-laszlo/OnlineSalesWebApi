namespace CarSales.Entities;

internal sealed class User
{
    internal int Id { get; private set; }
    internal string Email { get; private set; }

    private User(int id, string email)
    {
        Id = id;
        Email = email;
    }
}
