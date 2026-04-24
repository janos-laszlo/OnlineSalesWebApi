using Amazon.S3;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using VehicleSales.Entities.VehicleSale;
using static Common.Constants;

namespace VehicleSales.Queries;

public interface IGetUserVehicleSales
{
    Task<IReadOnlyList<VehicleSaleSummaryDto>> Execute(int userId, PagedRequest request, CancellationToken cancellation);
}

internal sealed class GetUserVehicleSales(
    IConfiguration configuration,
    IAmazonS3 r2Client) : IGetUserVehicleSales
{
    public async Task<IReadOnlyList<VehicleSaleSummaryDto>> Execute(int userId, PagedRequest request, CancellationToken cancellation)
    {
        var connectionString = configuration.GetConnectionString(ConfigKeys.ConnectionStringKey);
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellation);
        string query =
            $"""
            SELECT
                {nameof(VehicleSale.Id)},
                {nameof(Sale.Title)},
                {nameof(Money.AmountInCents)},
                {nameof(Money.Currency)},
                {nameof(VehicleDetails.VehicleModelId)},
                {nameof(VehicleDetails.VehicleVersion)},
                {nameof(VehicleDetails.VehicleManufacturingYear)},
                {nameof(VehicleDetails.Directory)},
                {nameof(VehicleDetails.PhotoKeys)}
            FROM {Tables.VehicleSales}
            WHERE {nameof(VehicleSale.SellerId)} = @SellerId
            ORDER BY {nameof(VehicleSale.Id)} ASC
            LIMIT @PageSize
            OFFSET @Offset;
            """;
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@SellerId", userId);
        command.Parameters.AddWithValue("@Offset", request.PageNumber * request.PageSize);
        command.Parameters.AddWithValue("@PageSize", request.PageSize);

        await using var reader = await command.ExecuteReaderAsync(cancellation);

        var baseUrl = $"{r2Client.Config.ServiceURL}/{configuration[R2Config.BucketNameKey]}/";

        var vehicleSales = new List<VehicleSaleSummaryDto>(request.PageSize);
        while (await reader.ReadAsync(cancellation))
        {
            vehicleSales.Add(
                new VehicleSaleSummaryDto(
                    Id: reader.GetInt32(0),
                    Title: reader.GetString(1),
                    AmountInCents: reader.GetUInt32(2),
                    Currency: (Currency)reader.GetInt32(3),
                    VehicleModelId: reader.GetInt32(4))
                {
                    VehicleVersion = reader.IsDBNull(5) ? null : reader.GetString(5),
                    VehicleManufacturingYear = reader.IsDBNull(6) ? null : reader.GetUInt16(6),
                    PhotoKeys = reader.IsDBNull(7) || reader.IsDBNull(8)
                        ? null
                        : reader.GetString(8).Split(VehicleSaleConfiguration.Separator)
                            .Select(key => $"{baseUrl}{reader.GetString(7)}/{key}")
                            .ToArray()
                });
        }

        return vehicleSales;
    }
}
