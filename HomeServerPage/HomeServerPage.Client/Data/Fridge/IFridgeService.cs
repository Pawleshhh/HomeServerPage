namespace HomeServerPage.Data.Fridge;

public interface IFridgeService
{
    Task<List<FridgeItem>> GetItemsAsync();

    Task AddItemAsync(FridgeItem item);
}
