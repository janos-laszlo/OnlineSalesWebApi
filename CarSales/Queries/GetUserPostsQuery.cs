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
        var carMakes = context.CarMakes
            .Include(cm => cm.CarModels)
            .AsSplitQuery()
            .ToArray();

        return context.UsersReadOnly
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                CarMakes = carMakes.Select(cm => new
                {
                    cm.Id,
                    cm.Name,
                    CarModels = cm.CarModels.Select(cmo => new
                    {
                        cmo.Id,
                        cmo.Name
                    })
                })
            })
            .FirstOrDefault();
    }
}
