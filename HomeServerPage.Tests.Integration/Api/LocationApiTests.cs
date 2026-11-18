using System.Net;
using System.Net.Http.Json;
using HomeServerPage.Data.Astronomy;
using HomeServerPage.Data.Locations;
using HomeServerPage.Tests.Integration.Infrastructure;

namespace HomeServerPage.Tests.Integration.Api;

public sealed class LocationApiTests
{
    [Test]
    public async Task InitialLocationsAreSeededAndFeatureSelectionsRemainIndependent()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var initialLocations =
            await client.GetFromJsonAsync<List<SavedLocation>>("/api/locations")
            ?? throw new InvalidOperationException("The saved locations response was empty.");
        var systemDefault = initialLocations.Single(location => location.IsSystemDefault);
        var szczecin = initialLocations.Single(location => location.Name == "Szczecin");
        var customLocation = await CreateLocationAsync(client, "Field", 53.2, 14.6);

        var weatherSelectionResponse = await client.PutAsJsonAsync(
            "/api/locations/selection/Weather",
            new LocationSelectionRequest(customLocation.Id));
        var astronomySelectionResponse = await client.PutAsJsonAsync(
            "/api/locations/selection/Astronomy",
            new LocationSelectionRequest(szczecin.Id));
        var weatherSelection = await client.GetFromJsonAsync<SavedLocation>(
            "/api/locations/selection/Weather");
        var astronomySelection = await client.GetFromJsonAsync<SavedLocation>(
            "/api/locations/selection/Astronomy");
        var deleteResponse = await client.DeleteAsync($"/api/locations/{customLocation.Id}");
        var weatherAfterDelete = await client.GetFromJsonAsync<SavedLocation>(
            "/api/locations/selection/Weather");

        Assert.That(systemDefault.Name, Is.EqualTo("Default"));
        Assert.That(weatherSelectionResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(astronomySelectionResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(weatherSelection!.Id, Is.EqualTo(customLocation.Id));
        Assert.That(astronomySelection!.Id, Is.EqualTo(szczecin.Id));
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(weatherAfterDelete!.Id, Is.EqualTo(systemDefault.Id));
    }

    [Test]
    public async Task LocationBoundariesAreAcceptedAndDefaultOrInvalidLocationsAreProtected()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var boundaryResponse = await client.PostAsJsonAsync(
            "/api/locations",
            new SavedLocation
            {
                Name = "Boundary",
                Latitude = 90,
                Longitude = 180,
                ElevationMeters = 0,
                TimeZoneId = "Etc/UTC"
            });
        var boundaryLocation = await boundaryResponse.Content.ReadFromJsonAsync<SavedLocation>();
        boundaryLocation!.Name = "Boundary updated";
        var updateBoundaryResponse = await client.PutAsJsonAsync(
            $"/api/locations/{boundaryLocation.Id}",
            boundaryLocation);
        var updatedBoundary = await client.GetFromJsonAsync<SavedLocation>(
            $"/api/locations/{boundaryLocation.Id}");
        var invalidResponse = await client.PostAsJsonAsync(
            "/api/locations",
            new SavedLocation
            {
                Name = "Invalid",
                Latitude = 90.01,
                Longitude = 0,
                TimeZoneId = "Etc/UTC"
            });
        var locations = await client.GetFromJsonAsync<List<SavedLocation>>("/api/locations");
        var systemDefault = locations!.Single(location => location.IsSystemDefault);
        var defaultDeleteResponse = await client.DeleteAsync($"/api/locations/{systemDefault.Id}");
        var whitespaceNameResponse = await client.PostAsJsonAsync(
            "/api/locations",
            new SavedLocation
            {
                Name = " ",
                Latitude = 0,
                Longitude = 0,
                TimeZoneId = "Etc/UTC"
            });

        Assert.That(boundaryResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(boundaryLocation!.Latitude, Is.EqualTo(90));
        Assert.That(boundaryLocation.Longitude, Is.EqualTo(180));
        Assert.That(updateBoundaryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(updatedBoundary!.Name, Is.EqualTo("Boundary updated"));
        Assert.That(invalidResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(defaultDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(whitespaceNameResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ObservationPointReferenceBlocksLocationDeletionUntilPointIsRemoved()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var location = await CreateLocationAsync(client, "Dark site", 52.1, 13.2);
        var pointResponse = await client.PostAsJsonAsync(
            "/api/observation-points",
            new ObservationPoint
            {
                Name = "North ridge",
                LocationId = location.Id,
                HorizonNorth = 12,
                HorizonNorthEast = 8,
                HorizonEast = 4,
                HorizonSouthEast = 2,
                HorizonSouth = 1,
                HorizonSouthWest = 3,
                HorizonWest = 6,
                HorizonNorthWest = 10
            });
        Assert.That(
            pointResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.Created),
            await pointResponse.Content.ReadAsStringAsync());
        var point = await pointResponse.Content.ReadFromJsonAsync<ObservationPoint>();
        var blockedDelete = await client.DeleteAsync($"/api/locations/{location.Id}");
        point!.Name = "Updated ridge";
        point.HorizonNorth = 15;
        var updatePointResponse = await client.PutAsJsonAsync(
            $"/api/observation-points/{point.Id}",
            point);
        var fetchedPoint = await client.GetFromJsonAsync<ObservationPoint>(
            $"/api/observation-points/{point!.Id}");
        var deletePoint = await client.DeleteAsync($"/api/observation-points/{point.Id}");
        var deleteLocation = await client.DeleteAsync($"/api/locations/{location.Id}");

        Assert.That(pointResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(updatePointResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(fetchedPoint!.Name, Is.EqualTo("Updated ridge"));
        Assert.That(fetchedPoint!.LocationId, Is.EqualTo(location.Id));
        Assert.That(fetchedPoint.Location!.Name, Is.EqualTo(location.Name));
        Assert.That(fetchedPoint.HorizonNorth, Is.EqualTo(15));
        Assert.That(blockedDelete.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(deletePoint.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(deleteLocation.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    private static async Task<SavedLocation> CreateLocationAsync(
        HttpClient client,
        string name,
        double latitude,
        double longitude)
    {
        var response = await client.PostAsJsonAsync(
            "/api/locations",
            new SavedLocation
            {
                Name = name,
                Latitude = latitude,
                Longitude = longitude,
                ElevationMeters = 10,
                TimeZoneId = "Etc/UTC"
            });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        return (await response.Content.ReadFromJsonAsync<SavedLocation>())!;
    }
}
