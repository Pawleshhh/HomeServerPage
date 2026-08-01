namespace HomeServerPage.Data.Fridge;

public interface IFridgeService
{
    public Task<FridgeItem?> GetItemAsync(int id);

    Task<List<FridgeItem>> GetItemsAsync();

    Task<FridgeItem> AddItemAsync(FridgeItem item);

    Task<bool> UpdateItemAsync(FridgeItem item);

    Task<bool> RemoveItemAsync(int id);
}
