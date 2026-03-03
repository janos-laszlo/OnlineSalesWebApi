namespace SalesWebApi.IntegrationTests.VehicleSalesEndpoints;

[Collection(VehicleSalesFixture.CollectionName)]
public sealed class GetVehicleMakesTests(VehicleSalesFixture fixture)
{
    [Fact]
    public async Task Returns_144_items()
    {
        // Act
        var result = await fixture.Client.GetFromJsonAsync<IReadOnlyList<string>>(VehicleSalesUris.VehicleMakesUri);
        Assert.NotNull(result);
        Assert.Equal(144, result!.Count);
    }
}
