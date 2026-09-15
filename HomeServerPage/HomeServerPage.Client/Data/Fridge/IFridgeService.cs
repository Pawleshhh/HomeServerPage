namespace HomeServerPage.Data.Fridge;

public interface IFridgeService
{
    public Task<FridgeItem?> GetItemAsync(int id);

    Task<List<FridgeItem>> GetItemsAsync();

    Task<List<FridgeItemTemplate>> GetTemplatesAsync();

    Task<FridgeItem> AddItemAsync(FridgeItem item, bool saveAsTemplate = false);

    Task<bool> UpdateItemAsync(FridgeItem item);

    Task<bool> RemoveItemAsync(int id);
}
