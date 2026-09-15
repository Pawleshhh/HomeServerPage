using System.Net.Http.Json;

namespace HomeServerPage.Data.Fridge;

public class FridgeHttpService(HttpClient httpClient) : IFridgeService
{

    public async Task<FridgeItem?> GetItemAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<FridgeItem>($"api/fridge/{id}") ?? null;
    }

    public async Task<List<FridgeItem>> GetItemsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<FridgeItem>>("api/fridge") ?? [];
    }

    public async Task<List<FridgeItemTemplate>> GetTemplatesAsync()
    {
        return await httpClient.GetFromJsonAsync<List<FridgeItemTemplate>>("api/fridge/templates") ?? [];
    }

    public async Task<FridgeItem> AddItemAsync(FridgeItem item, bool saveAsTemplate = false)
    {
        var uri = saveAsTemplate
            ? "api/fridge?saveAsTemplate=true"
            : "api/fridge";
        var resp = await httpClient.PostAsJsonAsync(uri, item);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<FridgeItem>())!;
    }

    public async Task<bool> UpdateItemAsync(FridgeItem item)
    {
        var resp = await httpClient.PutAsJsonAsync($"api/fridge/{item.Id}", item);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveItemAsync(int id)
    {
        var resp = await httpClient.DeleteAsync($"api/fridge/{id}");
        return resp.IsSuccessStatusCode;
    }
}
