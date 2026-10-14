# HomeServerPage

## Tablet dashboard

Configure automatic refresh in the server project's `appsettings.json` (and `appsettings.Development.json` when running in Development):

```json
"TabletDashboardRefresh": {
  "IntervalMinutes": 60,
  "ActiveFrom": "07:00",
  "ActiveUntil": "23:00"
}
```

The schedule uses Poland local time. `IntervalMinutes` must be between 1 and 1440 and is anchored to `ActiveFrom`; times use `HH:mm`, and `ActiveUntil` is exclusive and later than `ActiveFrom`. These defaults refresh the dashboard hourly from 07:00 through 22:00 and keep it idle overnight. Fridge changes saved from any device are pushed to open dashboards immediately through SignalR. The notification hub publishes named event types (`fridge.changed` today), so other services can publish and subscribe to their own notification types. Configuration changes take effect after restarting the app and reloading the dashboard.