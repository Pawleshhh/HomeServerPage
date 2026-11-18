using HomeServerPage.Client.Pages;
using HomeServerPage.Data.Fridge;

namespace HomeServerPage.Tests.Unit.Domain;

public sealed class FridgeRulesTests
{
    [Test]
    public void TemplateRoundTripTrimsNameAndUsesCalendarDays()
    {
        var added = new DateTime(2026, 4, 1, 21, 0, 0);
        var item = new FridgeItem(
            4,
            "  Milk  ",
            2,
            QuantityType.Unit,
            added,
            null,
            added.Date.AddDays(5).AddHours(7),
            TimeSpan.FromHours(48));

        var template = FridgeItemTemplate.FromFridgeItem(item);
        var created = template.ToFridgeItem(new DateTime(2026, 7, 2, 19, 30, 0));

        Assert.That(template.Name, Is.EqualTo("Milk"));
        Assert.That(template.ExpirationDaysAfterAdded, Is.EqualTo(5));
        Assert.That(created.Id, Is.Zero);
        Assert.That(created.AddedDate, Is.EqualTo(new DateTime(2026, 7, 2)));
        Assert.That(created.OpenDate, Is.Null);
        Assert.That(created.ExpirationDate, Is.EqualTo(new DateTime(2026, 7, 7)));
        Assert.That(created.TimeAfterOpen, Is.EqualTo(TimeSpan.FromHours(48)));
    }

    [Test]
    public void TemplateRejectsExpirationBeforeAddedDate()
    {
        var added = new DateTime(2026, 4, 2);
        var item = new FridgeItem(
            0,
            "Milk",
            1,
            QuantityType.Unit,
            added,
            null,
            added.AddDays(-1),
            null);

        Assert.Throws<ArgumentException>(() => FridgeItemTemplate.FromFridgeItem(item));
    }

    [Test]
    public void TemplateRejectsNegativeExpirationDaysWhenCreatingItem()
    {
        var template = new FridgeItemTemplate(
            0,
            "Milk",
            1,
            QuantityType.Unit,
            -1,
            null);

        Assert.Throws<InvalidOperationException>(
            () => template.ToFridgeItem(new DateTime(2026, 4, 2)));
    }

    [Test]
    public void OpenedItemUsesTheEarlierEffectiveExpiration()
    {
        var item = new FridgeItem(
            1,
            "Yogurt",
            1,
            QuantityType.Unit,
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 5),
            new DateTime(2026, 4, 20),
            TimeSpan.FromDays(2));

        Assert.That(
            FridgeLabels.GetEffectiveExpirationDate(item),
            Is.EqualTo(new DateTime(2026, 4, 7)));
        Assert.That(
            FridgeLabels.GetDaysLeftText(item, new DateTime(2026, 4, 7, 23, 59, 0)),
            Is.EqualTo("Today"));
        Assert.That(
            FridgeLabels.GetDaysLeftBadgeClass(item, new DateTime(2026, 4, 7)),
            Is.EqualTo("text-bg-danger"));
    }

    [Test]
    public void FormModelRejectsFractionalUnitQuantityAndInvalidDates()
    {
        var model = new FridgeItemModel
        {
            Name = "Milk",
            QuanitityValue = 1.5,
            QuantityType = QuantityType.Unit,
            AddedDate = new DateTime(2026, 4, 3),
            OpenDate = new DateTime(2026, 4, 2),
            ExpirationDate = new DateTime(2026, 4, 2)
        };
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            model,
            new System.ComponentModel.DataAnnotations.ValidationContext(model),
            results,
            validateAllProperties: true);

        Assert.That(isValid, Is.False);
        Assert.That(results.SelectMany(result => result.MemberNames), Does.Contain(nameof(model.QuanitityValue)));
        Assert.That(results.SelectMany(result => result.MemberNames), Does.Contain(nameof(model.OpenDate)));
        Assert.That(results.SelectMany(result => result.MemberNames), Does.Contain(nameof(model.ExpirationDate)));
    }
}
