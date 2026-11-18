using Bunit;
using HomeServerPage.Client.Shared;

namespace HomeServerPage.Tests.Unit.Components;

public sealed class ConfirmationDialogComponentTests
{
    [Test]
    public void ConfirmButtonCallsTheProvidedCallback()
    {
        var confirmed = false;
        using var context = new BunitContext();

        var component = context.Render<ConfirmationDialog>(parameters => parameters
            .Add(dialog => dialog.IsVisible, true)
            .Add(dialog => dialog.Title, "Remove item?")
            .Add(dialog => dialog.ConfirmText, "Remove")
            .Add(dialog => dialog.OnConfirm, () => confirmed = true));

        Assert.That(component.Find("[role=dialog]").TextContent, Does.Contain("Remove item?"));
        component.Find("button.btn-danger").Click();

        Assert.That(confirmed, Is.True);
    }
}
