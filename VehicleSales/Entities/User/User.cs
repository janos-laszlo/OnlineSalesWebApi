namespace VehicleSales.Entities.User;

internal sealed class User
{
    public int Id { get; private set; }
    public string Email { get; private set; }

    private User(int id, string email)
    {
        Id = id;
        Email = email;
    }
}
