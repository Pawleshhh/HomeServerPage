using HomeServerPage.Data.Fridge;

namespace HomeServerPage.Client.Pages;

public static class FridgeLabels
{
    public static string GetQuantityTypeLabel(QuantityType quantityType)
    {
        return quantityType switch
        {
            QuantityType.Weight => "(kg)",
            _ => string.Empty
        };
    }

    public static string GetDaysLeftText(FridgeItem item)
    {
        var effectiveExpirationDate = GetEffectiveExpirationDate(item);
        var daysLeft = (effectiveExpirationDate.Date - DateTime.Now.Date).Days;

        return daysLeft switch
        {
            < 0 => "Expired",
            0 => "Today",
            1 => "1 day",
            _ => $"{daysLeft} days"
        };
    }

    public static string GetDaysLeftBadgeClass(FridgeItem item)
    {
        var effectiveExpirationDate = GetEffectiveExpirationDate(item);
        var daysLeft = (effectiveExpirationDate.Date - DateTime.Now.Date).Days;

        return daysLeft switch
        {
            <= 0 => "text-bg-danger",
            <= 3 => "text-bg-warning",
            _ => "text-bg-success"
        };
    }

    public static DateTime GetEffectiveExpirationDate(FridgeItem item)
    {
        // An item is considered openable if it has a "time after open" value set.
        // Once opened, expiration is based on open date + time after open.
        // Until then (or if not openable), fall back to the item's expiration date.
        if (item.TimeAfterOpen.HasValue && item.OpenDate.HasValue)
        {
            return item.OpenDate.Value + item.TimeAfterOpen.Value;
        }

        return item.ExpirationDate;
    }
}
