using System.Net.Http.Json;

namespace HomeServerPage.Data.Fridge;

public class FridgeHttpService(HttpClient httpClient) : IFridgeService
{
    public async Task<List<FridgeItem>> GetItemsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<FridgeItem>>("api/fridge") ?? [];
    }

    public async Task AddItemAsync(FridgeItem item)
    {
        await httpClient.PostAsJsonAsync("api/fridge", item);
    }
}
