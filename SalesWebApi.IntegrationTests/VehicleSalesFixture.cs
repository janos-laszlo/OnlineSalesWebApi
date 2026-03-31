using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObjectUploadTracking;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using UserIdentity.Commands;
using VehicleSales;
using VehicleSales.Queries;

namespace SalesWebApi.IntegrationTests;

public sealed partial class VehicleSalesFixture : IDisposable
{
    [GeneratedRegex("image/")]
    private static partial Regex ImageContentType();
    internal const string CollectionName = "Vehicle Sales";
    private readonly WebApplicationFactory<Program> app;
    private readonly IServiceScope scope;
    private readonly IConfiguration configuration;
    private readonly IAmazonS3 s3Client;
    internal HttpClient Client { get; }
    internal HttpClient ExternalClient { get; }
    internal string AccessToken { get; }
    public Lazy<int> AnotherUsersVehicleSaleId { get; }

    public VehicleSalesFixture()
    {
        app = new WebApplicationFactory<Program>();
        scope = app.Services.CreateScope();
        configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        Client = app.CreateClient();
        ExternalClient = new HttpClient();
        AccessToken = RegisterAndLoginUser(new UserCredentialsDto(UserUtils.NextEmail, "Password1")).AccessToken;
        AnotherUsersVehicleSaleId = new Lazy<int>(CreateVehicleSaleForAnotherUser());
    }

    private Func<int> CreateVehicleSaleForAnotherUser()
    {
        return () =>
        {
            var otherUserToken = RegisterAndLoginUser(new UserCredentialsDto(UserUtils.NextEmail, "Password1")).AccessToken;
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
            {
                Content = new StringContent(
                    """
                    {        
                      "title": "2019 Audi A4 - Excellent Condition",
                      "description": "Well maintained 2019 Audi A4 with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
                      "amountInCents": 2699900,
                      "currency": "EUR",
                      "county": "Los Angeles",
                      "locality": "Santa Monica",
                      "vehicleModelId": 70,
                      "mileageInKilometers": 40000,
                      "horsePower": 248,
                      "vehicleVersion": "Premium Line",
                      "bodyType": "Sedan",
                      "engineVolumeInCm3": 1984,
                      "exteriorColor": "Mythos Black Metallic",
                      "interiorColor": "Gray",
                      "fuelType": "Petrol",
                      "vehicleManufacturingYear": 2019,
                      "vehicleNumberOfDoors": 4,
                      "vehicleCondition": "Used",
                      "gearboxType": "Automatic",
                      "steeringWheelSide": "Left",
                      "driveType": "FrontWheelDrive",
                      "numberOfSeats": 5,
                      "emissionStandard": "EURO6",
                      "hasServiceHistory": true,
                      "hasAccidentHistory": false,
                      "vin": "WAUENAF48KN123456",
                      "numberOfPreviousOwners": 1,
                      "batteryCapacityInKWh": 0,
                      "rangeInKilometers": 0,
                      "averageFuelConsumptionInLitersPer100Km": 7
                    }
                    """, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherUserToken);
            var response = Client.SendAsync(httpRequest, TestContext.Current.CancellationToken).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var location = response.Headers.Location?.OriginalString;
            Assert.NotNull(location);
            var idString = location.Split('/').Last();
            return int.Parse(idString);
        };
    }

    public void Dispose()
    {
        Client.Dispose();
        ExternalClient.Dispose();
        scope.Dispose();
        app.Dispose();
    }

    private TokenResponseDto RegisterAndLoginUser(UserCredentialsDto credentials)
    {
        var registrationResult = this.Client
            .PostAsJsonAsync(UserIdentityUris.RegisterUri, credentials)
            .GetAwaiter()
            .GetResult();
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = this.Client
            .PostAsJsonAsync(UserIdentityUris.LoginUri, credentials)
            .GetAwaiter()
            .GetResult();
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = loginResult.Content.ReadFromJsonAsync<TokenResponseDto>()
            .GetAwaiter()
            .GetResult();
        Assert.NotNull(body);
        return body;
    }


    internal Task<VehicleSaleFullDto?> GetVehicleSaleAsync(int id) =>
        Client.GetFromJsonAsync<VehicleSaleFullDto>($"{VehicleSalesUris.GetVehicleSaleById}{id}");

    internal async Task<int> CreateDefaultVehicleSaleAsync()
    {
        var requestBody =
        """
        {        
          "title": "2019 BMW 3 Series - Excellent Condition",
          "description": "Well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
          "amountInCents": 2499900,
          "currency": "EUR",
          "county": "Los Angeles",
          "locality": "Santa Monica",
          "vehicleModelId": 68,
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
          "averageFuelConsumptionInLitersPer100Km": 6
        }
        """;

        return await CreateVehicleSaleAsync(requestBody);
    }

    internal async Task<int> CreateVehicleSaleAsync(string requestBody)
    {
        var httpRequest = CreateVehicleSaleRequest(requestBody);
        var response = await Client.SendAsync(httpRequest, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location?.OriginalString;
        Assert.NotNull(location);
        var idString = location.Split('/').Last();

        if (requestBody.Contains("photoContentTypes"))
        {
            var responseBody = await response.Content.ReadFromJsonAsync<ObjectUploadTrackingDto>(TestContext.Current.CancellationToken);
            var objectUploadId = await UploadPhotos(responseBody);
            await ConfirmObjectUpload(objectUploadId);
        }

        return int.Parse(idString);
    }

    internal async Task UpdateVehicleSale(string? updatedPhotoKeys, int vehicleSaleId)
    {
        var updatedVehicleSale =
            $$"""
            {
                "county": "San Francisco",
                "locality": "Santa Monica1",
                "vehicleModelId": 80,
                "photos": {{updatedPhotoKeys ?? "null"}}
            }
            """;
        var updateRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{vehicleSaleId}")
        {
            Content = new StringContent(updatedVehicleSale, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", AccessToken);
        var response = await Client.SendAsync(
            updateRequest,
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadFromJsonAsync<ObjectUploadTrackingDto>(
            TestContext.Current.CancellationToken);
        Assert.Equal(vehicleSaleId, responseBody?.EntityId);

        var numberOfNewPhotos = ImageContentType().Count(updatedPhotoKeys ?? string.Empty);
        if (numberOfNewPhotos > 0)
        {
            Assert.NotNull(responseBody);
            Assert.NotNull(responseBody.ObjectUploadId);
            Assert.Equal(numberOfNewPhotos, responseBody.ObjectKeysAndTheirPresignedUploadUrls?.Count);
            var objectUploadId = await UploadPhotos(responseBody);
            await ConfirmObjectUpload(objectUploadId);
        }
        else
        {
            Assert.Null(responseBody?.ObjectUploadId);
        }
    }

    private async Task<int> UploadPhotos(ObjectUploadTrackingDto? responseBody)
    {
        Assert.NotNull(responseBody);
        Assert.True(responseBody.ObjectUploadId.HasValue);
        var presignedUrls = responseBody.ObjectKeysAndTheirPresignedUploadUrls;
        Assert.NotNull(presignedUrls);
        foreach (var (filename, presignedUrl) in presignedUrls)
        {
            ByteArrayContent content = new([]);
            content.Headers.ContentType = new MediaTypeHeaderValue(GetContentTypeFromFileExtension(filename));
            var uploadResponse = await ExternalClient.PutAsync(presignedUrl, content, TestContext.Current.CancellationToken);
            uploadResponse.EnsureSuccessStatusCode();
        }

        return responseBody.ObjectUploadId.Value;
    }

    private static string GetContentTypeFromFileExtension(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => throw new NotSupportedException($"File extension {extension} is not supported.")
        };
    }

    private async Task ConfirmObjectUpload(int objectUploadId)
    {
        var confirmRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{VehicleSalesUris.ConfirmObjectUpload}{objectUploadId}");
        confirmRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", this.AccessToken);

        var confirmResponse = await Client.SendAsync(
            confirmRequest,
            TestContext.Current.CancellationToken);
        Assert.True(confirmResponse.IsSuccessStatusCode);
    }

    internal HttpRequestMessage CreateVehicleSaleRequest(string requestBody)
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", AccessToken);
        return httpRequest;
    }

    internal async Task<string[]> GetObjectsInDirectory(
        string directory,
        CancellationToken cancellation)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = configuration[R2Config.BucketNameKey],
            Prefix = directory
        };

        var response = await s3Client.ListObjectsV2Async(request, cancellation);

        return response.S3Objects.Select(o => o.Key[(directory.Length + 1)..]).ToArray();
    }
}

[CollectionDefinition(VehicleSalesFixture.CollectionName)]
public class VehicleSalesCollection : ICollectionFixture<VehicleSalesFixture>
{ }
