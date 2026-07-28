namespace HomeServerPage.Data.Fridge;

public record FridgeItem(
    int Id, 
    string Name,
    double QuantityValue, QuantityType QuantityType,
    DateTime AddedDate, DateTime? OpenDate, DateTime ExpirationDate, TimeSpan? TimeAfterOpen);

public enum QuantityType
{
    Unit,
    Weight
}
