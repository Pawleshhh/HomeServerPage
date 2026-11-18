---
name: testing
description: Use when adding or updating unit, Blazor component, integration, or CI tests for HomeServerPage.
---

# HomeServerPage testing

## Test project map

- `HomeServerPage.Tests.Unit` references the server, client, and shared projects. It uses NUnit and Moq for unit tests, and bUnit for isolated Blazor component tests.
- `HomeServerPage.Tests.Integration` references the server project. It uses NUnit/Moq, `WebApplicationFactory<Program>`, and real SQLite databases and migrations.
- The projects are included in `HomeServerPage.slnx`. Run the full suite with `dotnet test HomeServerPage.slnx`; run one suite with `dotnet test .\HomeServerPage.Tests.Unit\HomeServerPage.Tests.Unit.csproj` or `dotnet test .\HomeServerPage.Tests.Integration\HomeServerPage.Tests.Integration.csproj`.
- The in-process API fixture is `HomeServerPage.Tests.Integration\Infrastructure\HomeServerPageWebApplicationFactory.cs`. Reusable deterministic helpers include `HomeServerPage.Tests.Unit\Infrastructure\StubHttpMessageHandler.cs`, `HomeServerPage.Tests.Unit\Infrastructure\FixedBrowserJsRuntime.cs`, and `HomeServerPage.Tests.Integration\Infrastructure\FixedDateTimeService.cs`.

## Choosing a test boundary

- Put deterministic domain calculations, validators, client services, notification behavior, and provider-adapter mapping in the unit project.
- Use bUnit 2 with `BunitContext.Render<T>()` for behavior-bearing pages and shared controls: register services before the first render, interact with the component, and verify visible states and service calls. Mock injected service interfaces with Moq. These are unit tests, not browser-driven UI or API-to-component integration tests.
- Use the integration project for HTTP API flows, persistence, migration/startup behavior, and service relationships that span databases. Exercise the app through `WebApplicationFactory<Program>` and real SQLite; do not mock `DbSet` queries.
- Keep test database files in the system temp directory, disable SQLite pooling when the fixture must delete files on Windows, and clean the fixture's files after each test.

## Determinism and assertions

- Never call live Weather or Public Transport providers from tests. Test adapter request construction and mapping with a deterministic `HttpMessageHandler`; replace provider service interfaces in integration/component tests. Tests must not require network access or credentials.
- Use a fixed `IDateTimeService` for server date boundaries and a fixed browser-clock helper for client components. Do not rely on the machine clock or wall-clock sleeps.
- Assert observable outcomes and boundary behavior—not compilation, trivial property passthrough, or the implementation itself. For calculations, use independently established expected values rather than copying the production formula into the assertion.
- Use Moq for external/service boundaries, clocks, and notification subscribers; prefer the real SQLite provider when database behavior is part of the assertion.
- Keep error statuses, persisted values, ordering, notifications, loading/error states, and past/today/future or min/max precision cases explicit in tests.
- `WebApplicationFactory<Program>` requires the server's top-level `Program` to remain public and partial. Date-dependent Calories and Exercise API validation must use `IDateTimeService`.

## Agreed behavior covered by tests

- A missing fridge item returns `404 Not Found`.
- Fridge create and update enforce the form constraints: a required name, non-negative quantity, whole-number unit quantities, expiration date not before added date, and open date not before added date.
- Weather and Public Transport tests use fakes; integration coverage is API/database level, while component rendering is isolated with bUnit.
- `TelescopeCalculator.CalculateAstrofotoFov` has an unused `additionalFocalLength` parameter; do not assert assumed behavior for it until its purpose is clarified.
- `dotnet test HomeServerPage.slnx` is the intended future CI entry point. CI workflow creation or changes are out of scope unless explicitly requested.
