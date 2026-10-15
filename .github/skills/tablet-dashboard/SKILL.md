---
name: tablet-dashboard
description: Use when changing the HomeServerPage tablet dashboard, its scheduled refresh, live clock, or realtime notifications.
---

# Tablet dashboard

## Product context

HomeServerPage is the user's personal website hosted on a local home server. It is used from smartphones and a touch monitor attached to a Raspberry Pi. It has a normal desktop/mobile mode and a compact `TabletDashboard` mode for a touch monitor of approximately 1020x600. The dashboard should be useful at that size without unnecessary scrolling.

## Confirmed behavior

- Dashboard data refreshes on a configurable schedule without reloading the whole page. The current default is every 60 minutes, active from 07:00 until 23:00 in Poland local time; the end time is exclusive and the interval is anchored to the start time.
- The schedule is configured in the server's `appsettings.json` under `TabletDashboardRefresh` (`IntervalMinutes`, `ActiveFrom`, `ActiveUntil`). The interval must be 1–1440 minutes and times use `HH:mm`. The defaults keep the dashboard idle overnight.
- Components should update through the existing refresh and notification events rather than forcing a page reload. Cross-device changes use the reusable SignalR notification mechanism; `fridge.changed` is an existing notification type.
- The dashboard date and clock should advance without requiring a manual page refresh. Use `IDateTimeService`; its client-side implementation must be synchronized with the server before reading values.
- Keep the established compact layout, existing light/dark theme behavior, and mode-specific navigation.

## Implementation map

- The dashboard page is `HomeServerPage/HomeServerPage.Client/Pages/Tablet/TabletDashboard.razor`.
- Dashboard schedule options and service are in `HomeServerPage/HomeServerPage.Client/Data/TabletDashboardRefreshOptions.cs` and `TabletDashboardRefreshService.cs`.
- The server binds and validates schedule options in `HomeServerPage/HomeServerPage/Program.cs`; the repository `README.md` documents the configuration.
- The client notification source and shared notification contracts are used by dashboard components; subscribe and unsubscribe using the existing lifecycle patterns.
- The live clock is part of `HomeServerPage/HomeServerPage.Client/Layout/TabletNavigation.razor`.

Before changing schedule semantics, timezone, active hours, refresh mechanisms, or the compact layout, ask if the requested behavior is not explicit. Keep configuration, validation, documentation, and client behavior in sync when changing the schedule.
