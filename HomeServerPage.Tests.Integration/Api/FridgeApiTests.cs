using System.Net;
using System.Net.Http.Json;
using HomeServerPage.Data.Fridge;
using HomeServerPage.Data.Notifications;
using HomeServerPage.Tests.Integration.Infrastructure;
using Moq;

namespace HomeServerPage.Tests.Integration.Api;

public sealed class FridgeApiTests
{
    [Test]
    public async Task CreateReadAndSaveTemplatePersistsValuesAndPublishesChange()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var item = CreateItem();

        var createResponse = await client.PostAsJsonAsync(
            "/api/fridge?saveAsTemplate=true",
            item);

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var created = await createResponse.Content.ReadFromJsonAsync<FridgeItem>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Id, Is.GreaterThan(0));
        Assert.That(created.Name, Is.EqualTo(item.Name));

        var getResponse = await client.GetAsync($"/api/fridge/{created.Id}");
        var persisted = await getResponse.Content.ReadFromJsonAsync<FridgeItem>();
        var templates = await client.GetFromJsonAsync<List<FridgeItemTemplate>>("/api/fridge/templates");

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(persisted, Is.EqualTo(created));
        Assert.That(templates, Has.Count.EqualTo(1));
        Assert.That(templates![0].Name, Is.EqualTo("Milk"));
        factory.NotificationPublisher.Verify(
            publisher => publisher.PublishAsync(
                RealtimeNotificationTypes.FridgeChanged,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task MissingItemReturnsNotFound()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/fridge/9999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateAndUpdateRejectInvalidFormValues()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var validItem = await CreateAndGetItemAsync(client);
        var invalidItems = new[]
        {
            validItem with { Name = " " },
            validItem with { QuantityValue = -1 },
            validItem with { QuantityValue = 1.5 },
            validItem with { QuantityType = (QuantityType)999 },
            validItem with { ExpirationDate = validItem.AddedDate.AddDays(-1) },
            validItem with { OpenDate = validItem.AddedDate.AddDays(-1) }
        };

        foreach (var invalidItem in invalidItems)
        {
            var createResponse = await client.PostAsJsonAsync("/api/fridge", invalidItem);
            var updateResponse = await client.PutAsJsonAsync(
                $"/api/fridge/{validItem.Id}",
                invalidItem);

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        var validWeightItem = validItem with
        {
            QuantityType = QuantityType.Weight,
            QuantityValue = 0.1
        };
        var validUpdateResponse = await client.PutAsJsonAsync(
            $"/api/fridge/{validItem.Id}",
            validWeightItem);

        Assert.That(validUpdateResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task UpdateAndDeleteChangePersistedItemState()
    {
        using var factory = new HomeServerPageWebApplicationFactory();
        using var client = factory.CreateClient();
        var item = await CreateAndGetItemAsync(client);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/fridge/{item.Id}",
            item with { Name = "Oat milk" });
        var updated = await client.GetFromJsonAsync<FridgeItem>($"/api/fridge/{item.Id}");
        var deleteResponse = await client.DeleteAsync($"/api/fridge/{item.Id}");
        var missingResponse = await client.GetAsync($"/api/fridge/{item.Id}");

        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(updated!.Name, Is.EqualTo("Oat milk"));
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(missingResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private static async Task<FridgeItem> CreateAndGetItemAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/fridge", CreateItem());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return (await response.Content.ReadFromJsonAsync<FridgeItem>())!;
    }

    private static FridgeItem CreateItem() => new(
        0,
        "Milk",
        1,
        QuantityType.Unit,
        new DateTime(2026, 11, 17),
        null,
        new DateTime(2026, 11, 22),
        TimeSpan.FromDays(3));
}
