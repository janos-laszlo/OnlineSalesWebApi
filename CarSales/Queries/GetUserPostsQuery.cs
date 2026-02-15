using Microsoft.EntityFrameworkCore;

namespace CarSales.Queries;

public interface IGetUserPostsQuery
{
    object? Get(int userId);
}

internal class GetUserPostsQuery(
    CarSalesDbContext context) : IGetUserPostsQuery
{
    public object? Get(int userId)
    {
        return context.UsersReadOnly
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email
            })
            .FirstOrDefault();
    }
}
