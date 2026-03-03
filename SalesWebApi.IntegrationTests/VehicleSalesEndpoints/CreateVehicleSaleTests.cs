using System.Net.Mime;

namespace SalesWebApi.IntegrationTests.VehicleSalesEndpoints;

[Collection(VehicleSalesFixture.CollectionName)]
public sealed class CreateVehicleSaleTests
{
    private readonly VehicleSalesFixture fixture;
    private readonly VerifySettings settings = new();

    public CreateVehicleSaleTests(VehicleSalesFixture fixture)
    {
        settings.ScrubLinesContaining("Authorization");
            settings.ScrubMember("entityId");
            settings.ScrubMember("Location");
            settings.ScrubMember("traceId");
        this.fixture = fixture;
    }

    [Fact]
    public async Task Created_for_valid_request()
    {
        // Arrange
        var requestWithoutPhotos = """
        {        
          "title": "2019 BMW 3 Series - Excellent Condition",
          "description": "Well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
          "amountInCents": 2499900,
          "currency": "EUR",
          "county": "Los Angeles",
          "locality": "Santa Monica",
          "vehicleModelId": 6248,
          "mileageInKilometers": 45000,
          "horsePower": 255,
          "vehicleVersion": "Sport Line",
          "bodyType": "Sedan",
          "engineVolumeInCm3": 1998,
          "exteriorColor": "Alpine White",
          "interiorColor": "Black",
          "fuelType": "Petrol",
          "vehicleManufacturingYear": 2019,
          "vehicleNumberOfDoors": 4,
          "vehicleCondition": "Used",
          "gearboxType": "Automatic",
          "steeringWheelSide": "Left",
          "driveType": "RearWheelDrive",
          "numberOfSeats": 5,
          "emissionStandard": "EURO6",
          "hasServiceHistory": true,
          "hasAccidentHistory": false,
          "vin": "WBA5R1C50KAK12345",
          "numberOfPreviousOwners": 1,
          "batteryCapacityInKWh": 11000,
          "rangeInKilometers": 1000,
          "averageFuelConsumptionInLitersPer100Km": 6,
          "averageBatteryConsumptionInKWhPer100Km": 90,
          "massInKg": 1520,
          "maximumLoadInKg": 480
        }
        """;
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
        {
            Content = new StringContent(
                requestWithoutPhotos, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", fixture.AccessToken);

        // Act
        var response = await fixture.Client.SendAsync(httpRequest, CancellationToken.None);

        // Assert
        await Verify(response, settings);
    }

    [Fact]
    public async Task BadRequest_for_missing_required_fields()
    {
        // Arrange
        var requestWithoutRequiredFields =
            """
            {
              "numberOfPreviousOwners": 1,
              "batteryCapacityInKWh": 11000,
              "rangeInKilometers": 1000,
              "averageFuelConsumptionInLitersPer100Km": 6,
              "averageBatteryConsumptionInKWhPer100Km": 90,
              "massInKg": 1520,
              "maximumLoadInKg": 480
            }
            """;
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
        {
            Content = new StringContent(
                requestWithoutRequiredFields, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", fixture.AccessToken);

        // Act
        var response = await fixture.Client.SendAsync(httpRequest, CancellationToken.None);

        // Assert
        await Verify(response, settings);
    }
}
