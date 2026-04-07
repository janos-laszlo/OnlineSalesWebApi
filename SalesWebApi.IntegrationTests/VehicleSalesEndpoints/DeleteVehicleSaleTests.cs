using System.Net;
using System.Net.Http.Headers;

namespace SalesWebApi.IntegrationTests.VehicleSalesEndpoints;

[Collection(VehicleSalesFixture.CollectionName)]
public sealed class DeleteVehicleSaleTests(VehicleSalesFixture fixture)
{
    private readonly VehicleSalesFixture fixture = fixture;

    [Fact]
    public async Task NoContent_for_valid_request()
    {
        // Arrange
        var vehicleSaleIdToDelete = await fixture.CreateDefaultVehicleSaleAsync();

        // Act
        var deleteStatusCode = await DeleteVehicleSale(vehicleSaleIdToDelete);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteStatusCode);
    }

    [Fact]
    public async Task NotFound_for_nonexistent_vehicle_sale()
    {
        // Arrange
        var nonexistentVehicleSaleId = 9999;

        // Act
        var deleteStatusCode = await DeleteVehicleSale(nonexistentVehicleSaleId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, deleteStatusCode);
    }

    [Fact]
    public async Task Forbidden_for_another_users_vehicle_sale()
    {
        // Act
        var deleteStatusCode = await DeleteVehicleSale(fixture.AnotherUsersVehicleSaleId.Value);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, deleteStatusCode);
    }

    private async Task<HttpStatusCode> DeleteVehicleSale(int vehicleSaleId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Delete, $"/vehicle-sales/{vehicleSaleId}");

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", fixture.AccessToken);

        var response = await fixture.Client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        return response.StatusCode;
    }
}
