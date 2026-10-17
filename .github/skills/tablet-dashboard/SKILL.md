---
name: tablet-dashboard
description: Use when changing the HomeServerPage tablet dashboard, its scheduled refresh, live clock, or realtime notifications.
---

# Tablet dashboard

## Product context

HomeServerPage is the user's personal website hosted on a local home server. It is used from smartphones and a touch monitor attached to a Raspberry Pi. It has a normal desktop/mobile mode and a compact `TabletDashboard` mode for a touch monitor of approximately 1024x600. Keep dashboard widgets within that viewport without vertical or horizontal scrolling where practical.

## Scheduled refresh

- Refresh each widget on its own configurable interval without reloading the whole page. The shared default active window is 07:00–23:00 Poland local time (`ActiveUntil` is exclusive), and each widget's cadence is anchored to `ActiveFrom`.
- Configure the server's `appsettings.json` under `TabletDashboardRefresh`:

  ```json
  "TabletDashboardRefresh": {
    "ActiveFrom": "07:00",
    "ActiveUntil": "23:00",
    "Widgets": {
      "FridgeIntervalMinutes": 60,
      "PublicTransportIntervalMinutes": 20,
      "WeatherIntervalMinutes": 60,
      "AstronomyIntervalMinutes": 1440
    }
  }
  ```

- Each widget interval must be 1–1440 minutes; active-range times use `HH:mm`. Defaults refresh fridge hourly, public transport every 20 minutes, weather hourly, and astronomy once daily.
- Scheduled refresh is not a substitute for cross-device events. Keep SignalR notifications immediate and independent of each widget's scheduled interval and active hours. For example, `fridge.changed` updates open fridge widgets.
- SignalR does not replay missed notifications. When a connection is restored, refresh/resynchronize subscribed widget data so the UI recovers changes missed while disconnected.
- Update the visible UI after loading refreshed data, and unsubscribe event handlers using the existing component lifecycle patterns.

## Clock and astronomy

- The dashboard date and clock should advance without requiring a manual page refresh. Use `IDateTimeService`; its client-side implementation must be synchronized with the server before reading values.
- The astronomy card and Astronomy observations page share `MoonPhase`; keep their display behavior consistent.
- `MoonPhase` uses a NASA SVS image asset for supported dates, not a NASA API. Select the frame using the supplied UTC observation timestamp. Keep the drawn moon as a fallback for unsupported dates or image-load failures. If deriving the current timestamp in the client, sync `IDateTimeService` first.
- Preserve light/dark theme support and mode-specific navigation. Keep weather, astronomy, and public-transport widgets compact and usable at the target resolution.

## Implementation map

- The dashboard page is `HomeServerPage/HomeServerPage.Client/Pages/Tablet/TabletDashboard.razor`.
- Dashboard schedule options and service are in `HomeServerPage/HomeServerPage.Client/Data/TabletDashboardRefreshOptions.cs` and `TabletDashboardRefreshService.cs`.
- The server binds and validates schedule options in `HomeServerPage/HomeServerPage/Program.cs`; the repository `README.md` documents the configuration.
- Widget refresh handlers are in the widget `.razor.cs` partials. The client notification source and shared notification contracts are used by dashboard components.
- The live clock is part of `HomeServerPage/HomeServerPage.Client/Layout/TabletNavigation.razor`.
- The shared moon display is `HomeServerPage/HomeServerPage.Client/Pages/Astronomy/MoonPhase.razor` and `.razor.cs`, used by `AstronomyWidget` and `AstronomyObservationsTab`.

Before changing schedule semantics, timezone, active hours, widget cadence, refresh mechanisms, or compact layout, ask if the requested behavior is not explicit. Keep configuration, validation, documentation, and client behavior in sync when changing the schedule.
