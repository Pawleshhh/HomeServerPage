using HomeServerPage.Data.Fridge;
using Microsoft.AspNetCore.Mvc;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FridgeController(IFridgeService fridgeService) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<ActionResult<FridgeItem>> GetItem(int id)
    {
        var item = await fridgeService.GetItemAsync(id);
        return Ok(item);
    }

    [HttpGet]
    public async Task<ActionResult<List<FridgeItem>>> GetItems()
    {
        var items = await fridgeService.GetItemsAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<FridgeItem>> AddItem([FromBody] FridgeItem item)
    {
        var created = await fridgeService.AddItemAsync(item);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] FridgeItem item)
    {
        var updated = await fridgeService.UpdateItemAsync(item with { Id = id });
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var removed = await fridgeService.RemoveItemAsync(id);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}
