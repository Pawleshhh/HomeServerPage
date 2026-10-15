---
name: calories-tracker
description: Use when changing the calories tracker page, its food history, nutrition data, charts, or tablet presentation.
---

# Calories tracker

## Confirmed product requirements

- This is a normal page, not a widget on the main tablet dashboard. It is available through normal navigation and tablet navigation.
- Users can keep a reusable list of foods they commonly eat and log food for a selected day. A one-off/custom food must be recorded in history but must not become a reusable food automatically.
- A food requires a name and calorie value. Carbohydrates, protein, and fat are optional. Do not assume extra nutrition fields, units, or behavior that the user has not specified; ask when a product decision is unresolved.
- Calculate daily and weekly calories and show historical statistics. Keep the calorie chart's kcal axis and selected-period value understandable.
- The page is used on desktop, mobile, and the approximately 1020x600 touch display. Preserve responsive behavior, light/dark theme support, and accurate loading, error, and refresh states.

## Tablet presentation

- Prioritize the selected day's log and calorie information over weekly statistics and goal editing.
- Keep the page compact enough to minimize whole-page scrolling. Long food logs may scroll within their own panel.
- Keep the add-food form compact; in tablet mode it opens as an overlay rather than permanently consuming dashboard space.
- If a daily goal already exists, its progress may remain visible in tablet mode; do not show the weekly summary or goal editor there unless the user asks.
- Keep related form values together in compact rows and allow them to wrap on narrower screens.

## Implementation map

- `/calories` and `/tablet-dashboard/calories` use the shared page component `HomeServerPage/HomeServerPage.Client/Pages/Calories/Calories.razor`, with `CaloriesPage.razor` and `TabletCalories.razor` selecting the mode.
- Normal and tablet navigation are separate. Preserve both when changing routes or page availability.
- Client contracts and HTTP access are under `HomeServerPage/HomeServerPage.Client/Data/Calories`; server behavior and persistence are under `HomeServerPage/HomeServerPage/Data/Calories` and `Controllers/CaloriesController.cs`.
- The tracker has its own SQLite `CaloriesDbContext` and migrations. Follow the existing EF Core migration and service patterns for persistence changes.

Ask before changing calorie-log semantics, reusable-food behavior, totals, chart meaning, or tablet priorities when the request does not specify the intended behavior.
