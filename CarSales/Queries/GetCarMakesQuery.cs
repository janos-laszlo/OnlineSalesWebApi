using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;

namespace CarSales.Queries;

public interface IGetCarMakesQuery
{
    Task<IReadOnlyList<string>> Get(CancellationToken cancellationToken);
}

internal class GetCarMakesQuery(
    CarSalesDbContext context,
    IAmazonS3 amazonS3) : IGetCarMakesQuery
{
    public async Task<IReadOnlyList<string>> Get(CancellationToken cancellationToken)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = "car-sales",
            Key = "log.txt",
            Verb = HttpVerb.PUT,
            Expires = DateTime.Now.AddMinutes(5)
        };

        var response = await amazonS3.GetPreSignedURLAsync(request);

        return await context.CarMakes
            .Select(carMake => carMake.Name)
            .ToArrayAsync(cancellationToken);
    }
}
