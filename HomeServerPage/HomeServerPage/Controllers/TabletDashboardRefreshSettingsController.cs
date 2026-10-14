using HomeServerPage.Client.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HomeServerPage.Controllers;

[ApiController]
[Route("api/tablet-dashboard-refresh-settings")]
public sealed class TabletDashboardRefreshSettingsController(
    IOptions<TabletDashboardRefreshOptions> options) : ControllerBase
{
    [HttpGet]
    public ActionResult<TabletDashboardRefreshOptions> GetSettings() => Ok(options.Value);
}
