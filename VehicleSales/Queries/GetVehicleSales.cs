using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using VehicleSales.Entities.VehicleSale;
using static Common.Constants;

namespace VehicleSales.Queries;

public interface IGetVehicleSales
{
    Task<IReadOnlyList<VehicleSaleSummaryDto>> Execute(PagedRequest request, CancellationToken cancellation);
}

internal sealed class GetVehicleSales(
    IConfiguration configuration) : IGetVehicleSales
{
    public async Task<IReadOnlyList<VehicleSaleSummaryDto>> Execute(PagedRequest request, CancellationToken cancellation)
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
            ORDER BY {nameof(VehicleSale.Id)} ASC
            LIMIT @PageSize
            OFFSET @Offset;
            """;
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Offset", request.PageNumber * request.PageSize);
        command.Parameters.AddWithValue("@PageSize", request.PageSize);

        await using var reader = await command.ExecuteReaderAsync(cancellation);

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
                    Directory = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PhotoKeys = reader.IsDBNull(8) ? null : reader.GetString(8)?.Split(VehicleSaleConfiguration.Separator)
                });
        }

        return vehicleSales;
    }
}

public sealed record VehicleSaleSummaryDto(
    int Id,

    [property: Description("The display title of the sale listing.")]
    [property: MinLength(SaleTitle.MinLength), MaxLength(SaleTitle.MaxLength)]
    string Title,

    [property: Description("Sale price in cents (e.g. 150000 = $1,500).")]
    uint AmountInCents,

    [property: Description("Currency of the sale price.")]
    Currency Currency,

    [property: Description("ID of the vehicle model.")]
    int VehicleModelId)
{
    [Description("Additional info to the vehicle model")]
    [MinLength(Entities.VehicleSale.VehicleVersion.MinLength)]
    [MaxLength(Entities.VehicleSale.VehicleVersion.MaxLength)]
    public string? VehicleVersion { get; init; }

    [Description($"Year the vehicle was manufactured. Must be >= 1880 and <= current year")]
    public ushort? VehicleManufacturingYear { get; init; }

    public string? Directory { get; init; }
    public IReadOnlyList<string>? PhotoKeys { get; set; }
}
