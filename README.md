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

## Saved locations

Manage saved locations from **Settings → Locations**. Each record contains a name, latitude, longitude, elevation, and a time-zone ID supported by the server. Weather and Astronomy keep independent selections; each feature's dashboard widget shares its page's selection. New installations start both features at Szczecin. The protected `Default` fallback uses coordinates `0, 0, 0` and `Etc/UTC`.

The location catalog and per-feature selections are stored in `Database/locations.db` (or `/app/Database/locations.db` in the container). Back up this file together with `astronomy.db`: Observation Points keep their horizon profiles in the Astronomy database and refer to locations in the location database. The application preserves those links and blocks deletion of locations still used by Observation Points, but SQLite cannot enforce a foreign key across the two database files.

`WeatherApi:Location` (Docker: `WEATHER_API_LOCATION`, default `Szczecin`) is used only when initializing the Weather selection for the first time. If it names a location not already saved, the server resolves it through the weather provider and saves its coordinates and time zone. Later Weather selection changes are persisted and are not overridden by this setting. The weather API key remains server-side in `WEATHER_API`.