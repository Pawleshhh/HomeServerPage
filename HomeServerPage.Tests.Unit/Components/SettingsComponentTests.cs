using Bunit;
using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Client.Pages.Settings;
using HomeServerPage.Data.Locations;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HomeServerPage.Tests.Unit.Components;

public sealed class SettingsComponentTests
{
    [Test]
    public void SelectingSystemDefaultDisablesEditingAndDeletion()
    {
        var defaultLocation = new SavedLocation
        {
            Id = 1,
            Name = "Default",
            TimeZoneId = "Etc/UTC",
            IsSystemDefault = true
        };
        var service = new Mock<ILocationService>();
        service
            .Setup(item => item.GetLocationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { defaultLocation });
        using var context = new BunitContext();
        context.Services.AddSingleton(service.Object);

        var component = context.Render<Settings>();
        component.Find(".list-group-item").Click();

        Assert.That(component.Find("#saved-location-name").HasAttribute("disabled"), Is.True);
        Assert.That(component.Find("button[type=submit]").HasAttribute("disabled"), Is.True);
        Assert.That(component.Find("button.btn-outline-danger").HasAttribute("disabled"), Is.True);
    }

    [Test]
    public void SavesValidLocationAndDisplaysItInTheSavedList()
    {
        var locations = new List<SavedLocation>();
        var service = new Mock<ILocationService>();
        service
            .Setup(item => item.GetLocationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => (IReadOnlyList<SavedLocation>)locations.ToArray());
        service
            .Setup(item => item.SaveLocationAsync(
                It.IsAny<SavedLocation>(),
                It.IsAny<CancellationToken>()))
            .Returns((SavedLocation location, CancellationToken _) =>
            {
                var saved = new SavedLocation
                {
                    Id = 7,
                    Name = location.Name,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    ElevationMeters = location.ElevationMeters,
                    TimeZoneId = location.TimeZoneId
                };
                locations.Add(saved);
                return Task.FromResult(saved);
            });
        using var context = new BunitContext();
        context.Services.AddSingleton(service.Object);

        var component = context.Render<Settings>();
        component.Find("#saved-location-name").Change("North ridge");
        component.Find("#saved-location-latitude").Change("53");
        component.Find("#saved-location-longitude").Change("14");
        component.Find("#saved-location-elevation").Change("25");
        component.Find("#saved-location-time-zone").Change("Etc/UTC");
        component.Find("form").Submit();

        component.WaitForAssertion(() =>
            Assert.That(component.Markup, Does.Contain("North ridge")));
        service.Verify(
            item => item.SaveLocationAsync(
                It.Is<SavedLocation>(location =>
                    location.Name == "North ridge" &&
                    location.Latitude == 53 &&
                    location.Longitude == 14 &&
                    location.ElevationMeters == 25 &&
                    location.TimeZoneId == "Etc/UTC"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
