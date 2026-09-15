namespace HomeServerPage.Data.Fridge;

public record FridgeItemTemplate(
    int Id,
    string Name,
    double QuantityValue,
    QuantityType QuantityType,
    int ExpirationDaysAfterAdded,
    TimeSpan? TimeAfterOpen)
{
    public static FridgeItemTemplate FromFridgeItem(FridgeItem item)
    {
        var name = NormalizeName(item.Name);
        var expirationDaysAfterAdded = (item.ExpirationDate.Date - item.AddedDate.Date).Days;

        if (expirationDaysAfterAdded < 0)
        {
            throw new ArgumentException(
                "Expiration date cannot be earlier than added date.",
                nameof(item));
        }

        return new(
            0,
            name,
            item.QuantityValue,
            item.QuantityType,
            expirationDaysAfterAdded,
            item.TimeAfterOpen);
    }

    public FridgeItem ToFridgeItem(DateTime now)
    {
        var name = NormalizeName(Name);
        if (ExpirationDaysAfterAdded < 0)
        {
            throw new InvalidOperationException(
                "Expiration days after added cannot be negative.");
        }

        var addedDate = now.Date;
        return new(
            0,
            name,
            QuantityValue,
            QuantityType,
            addedDate,
            null,
            addedDate.AddDays(ExpirationDaysAfterAdded),
            TimeAfterOpen);
    }

    public static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }
}
