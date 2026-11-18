using Bunit;
using HomeServerPage.Client.Pages.Fridge;
using HomeServerPage.Data;
using HomeServerPage.Data.Fridge;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HomeServerPage.Tests.Unit.Components;

public sealed class FridgeComponentTests
{
    [Test]
    public void AddingAnItemUsesTheServiceAndShowsUpdatedStatus()
    {
        var items = new List<FridgeItem>();
        var fridgeService = new Mock<IFridgeService>();
        fridgeService
            .Setup(service => service.GetItemsAsync())
            .ReturnsAsync(() => items.ToList());
        fridgeService
            .Setup(service => service.GetTemplatesAsync())
            .ReturnsAsync([]);
        fridgeService
            .Setup(service => service.AddItemAsync(
                It.IsAny<FridgeItem>(),
                It.IsAny<bool>()))
            .Returns((FridgeItem item, bool _) =>
            {
                var created = item with { Id = 1 };
                items.Add(created);
                return Task.FromResult(created);
            });
        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService
            .Setup(service => service.SyncAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        dateTimeService
            .SetupGet(service => service.Now)
            .Returns(new DateTime(2026, 11, 18, 12, 0, 0));
        using var context = new BunitContext();
        context.Services.AddSingleton(fridgeService.Object);
        context.Services.AddSingleton(dateTimeService.Object);

        var component = context.Render<Fridge>();
        component.Find("#fridge-name").Input("Eggs");
        component.Find("form.fridge-form").Submit();

        component.WaitForAssertion(() =>
        {
            Assert.That(component.Markup, Does.Contain("Eggs added to the fridge"));
            Assert.That(component.Markup, Does.Contain("Eggs"));
        });
        fridgeService.Verify(
            service => service.AddItemAsync(
                It.Is<FridgeItem>(item => item.Name == "Eggs"),
                true),
            Times.Once);
        dateTimeService.Verify(
            service => service.SyncAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
