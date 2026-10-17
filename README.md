# HomeServerPage

## Tablet dashboard

Configure automatic refresh in the server project's `appsettings.json` (and `appsettings.Development.json` when running in Development):

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

The schedule uses Poland local time. Each widget interval must be between 1 and 1440 minutes and is anchored to `ActiveFrom`; times use `HH:mm`, and `ActiveUntil` is exclusive and later than `ActiveFrom`. The defaults refresh public transport every 20 minutes, weather hourly, astronomy once a day, and the fridge summary hourly between 07:00 and 23:00. The shared active range still keeps scheduled refreshes idle overnight. Fridge changes saved from any device are pushed to open dashboards immediately through SignalR, regardless of the scheduled refresh range. The notification hub publishes named event types (`fridge.changed` today), so other services can publish and subscribe to their own notification types. Configuration changes take effect after restarting the app and reloading the dashboard.