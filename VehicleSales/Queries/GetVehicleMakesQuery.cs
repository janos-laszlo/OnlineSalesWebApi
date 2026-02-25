using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;

namespace VehicleSales.Queries;

public interface IGetVehicleMakesQuery
{
    Task<IReadOnlyList<string>> Get(CancellationToken cancellationToken);
}

internal class GetVehicleMakesQuery(
    VehicleSalesDbContext context,
    IAmazonS3 amazonS3) : IGetVehicleMakesQuery
{
    public async Task<IReadOnlyList<string>> Get(CancellationToken cancellationToken)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = BucketNames.VehicleSales,
            Key = "log.txt",
            Verb = HttpVerb.PUT,
            Expires = DateTime.Now.AddMinutes(5)
        };

        var response = await amazonS3.GetPreSignedURLAsync(request);

        return await context.VehicleMakes
            .Select(vehicleMake => vehicleMake.Name)
            .ToArrayAsync(cancellationToken);
    }
}
