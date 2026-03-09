namespace SalesWebApi.IntegrationTests.VehicleSalesEndpoints;

[Collection(VehicleSalesFixture.CollectionName)]
public sealed class GetVehicleModelsTests(VehicleSalesFixture fixture)
{
    [Fact]
    public async Task Returns_170_models_for_Toyota()
    {
        // Arrange

        // Act
        var result = await fixture.Client.GetFromJsonAsync<IReadOnlyList<string>>(
            $"{VehicleSalesUris.VehicleModelsUri}?makeName=Toyota",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(170, result!.Count);
    }
}
