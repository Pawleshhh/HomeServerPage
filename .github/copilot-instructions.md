# HomeServerPage agent instructions

## Project context

HomeServerPage is a personal website running on a local home server. It is commonly accessed from a smartphone and from a touch monitor attached to a Raspberry Pi; remote smartphone access uses Tailscale. It is a .NET 10 Blazor application with separate server, WebAssembly client, and shared projects:

- `HomeServerPage/HomeServerPage` contains the ASP.NET Core host, API controllers, server-side services, SQLite `DbContext`s, EF Core migrations, and SignalR hub.
- `HomeServerPage/HomeServerPage.Client` contains Blazor pages, layouts, UI components, browser-side services, and HTTP clients.
- `HomeServerPage.Shared` contains code shared between the server and client.

Follow the existing service and component patterns when changing behavior. Keep server persistence behind the existing service abstractions and expose client access through the existing HTTP-service patterns. Add or update EF Core migrations when changing a database model. Use `IDateTimeService` for application date/time and follow its synchronization lifecycle in the WebAssembly client; do not read its client-side values before they have been synced.

## Product and interaction expectations

- The site has a normal responsive mode and a compact `TabletDashboard` mode. The primary touch display is approximately 1024x600.
- Keep normal and tablet navigation, layouts, and page behavior consistent with the existing mode-specific patterns.
- Keep related form labels and controls vertically aligned: use consistent label space and control heights within a row, and place loading/error/help messages so they do not shift one field relative to another. Preserve alignment when the layout wraps.
- Preserve light and dark theme support, loading/error states, and correct refresh behavior when changing pages or widgets.
- Keep project work on the active branch. Do not create another branch, stash changes, or commit unless the user explicitly asks.
- For new product or UX work, ask about unresolved behavior, scope, or layout choices before implementation instead of silently choosing defaults. Ask focused questions and wait for answers where the decision affects user-visible behavior. Do not ask about choices already answered by the request or established code.
- Treat current code and explicit user decisions as authoritative. Session-derived context in the linked skills is a baseline, not a reason to ignore a newer request or changed implementation. Update these guidance files when the user establishes a durable new requirement.

## Implementation and verification

- Keep changes focused and consistent with the existing project structure and conventions.
- Always qualify references to private instance fields with `this.` (for example, `this.isLoading`), including in Razor component code and code-behind files.
- Use the existing SQLite, SignalR, date/time, configuration, and client-service abstractions rather than introducing parallel mechanisms.
- Keep credentials and private configuration out of committed files. The server currently supports supplying the weather API key through the `WEATHER_API` configuration key.
- Build the solution with `dotnet build HomeServerPage.slnx` after code changes; run relevant tests when present.

## Testing

- `HomeServerPage.Tests.Unit` contains domain/service unit tests and bUnit component tests. It uses NUnit and Moq; use bUnit for isolated rendering and component interactions, not browser automation.
- `HomeServerPage.Tests.Integration` contains server API tests. It uses `WebApplicationFactory<Program>`, real SQLite databases/migrations, and test databases under the system temp directory.
- Run all tests with `dotnet test HomeServerPage.slnx`. To run one suite, use `dotnet test .\HomeServerPage.Tests.Unit\HomeServerPage.Tests.Unit.csproj` or `dotnet test .\HomeServerPage.Tests.Integration\HomeServerPage.Tests.Integration.csproj`.
- Test observable behavior, state changes, domain boundaries, persistence, API responses, and rendered behavior—not properties or wiring that compilation already verifies. Assert independent expected values instead of repeating the implementation formula in the assertion.
- Use Moq for service boundaries, clocks, and notification subscribers. Do not mock EF Core `DbSet` query behavior; use the integration SQLite fixture for persistence.
- Never call live Weather or Public Transport providers from tests. Use deterministic HTTP handlers for provider adapters and fake service registrations for API/component tests; tests must not require API keys or network access.
- Use `IDateTimeService` or the test clock helpers for date-sensitive behavior. The client clock must be synced before its values are read.
- Keep API integration tests at the HTTP/service/database boundary. Component tests use mocked services in bUnit; browser-driven UI tests are not part of the current test setup.
- The test projects are ready for a future CI job, but do not add or change CI workflows unless requested.

## Focused skills

- For dashboard, scheduled refresh, live clock, or realtime notification work, read [the tablet dashboard skill](skills/tablet-dashboard/SKILL.md).
- For calories tracker work, read [the calories tracker skill](skills/calories-tracker/SKILL.md).
- For unit, component, integration, or CI test work, read [the testing skill](skills/testing/SKILL.md).
