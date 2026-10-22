---
name: calories-tracker
description: Use when changing the calories tracker page, its food history, nutrition data, charts, or tablet presentation.
---

# Calories tracker

## Food and entry behavior

- This is a normal page, not a widget on the main tablet dashboard. It is available through normal navigation and tablet navigation.
- Reusable foods store whole-number kcal and optional whole-gram carbohydrates, protein, and fat per generic serving. Food names need not be unique; reusable foods can be edited or deleted.
- Logging a reusable food defaults to one serving. The positive serving multiplier accepts up to two decimal places; scale kcal and recorded macros by it and round each result to the nearest whole number.
- A custom food records the nutrition for the exact amount eaten and is not added to the reusable-food list.
- A logged entry is a snapshot. Editing or deleting a reusable food must not retroactively change existing log entries.
- The daily log is a flat list ordered by time, without meal categories. Dates and times use the device's local clock; the time is editable. Users may log entries for any past date through today, but not future dates.
- Entries can be edited or deleted whenever their day is viewed, including past days. Preserve any snapshot data needed so saved-food edits do not alter logged history.
- Keep loading, error, save, and refresh states accurate. Use the existing client-service, server-service, SQLite, and notification patterns.

## Totals, goal, and history

- Calculate daily and weekly calorie totals. Include recorded carbohydrates, protein, and fat in daily and weekly macro totals; do not treat missing macros as zero-valued recorded data.
- The daily calorie goal is a single global value that applies across days, not a per-day setting.
- History charts show calories only and provide numeric summaries. Default ranges are the last 7 days, 12 Monday–Sunday calendar weeks, and 12 calendar months.
- Show a labeled kcal axis and the selected period's calorie value when a chart point is selected.

## Responsive and tablet presentation

- The page is used on desktop, mobile, and the approximately 1024x600 touch display. Preserve responsive behavior, light/dark theme support, and mode-specific navigation.
- Tablet mode prioritizes the selected day's log and calorie information over weekly statistics and goal editing.
- Keep the tablet page compact to minimize whole-page scrolling. Long food logs may scroll within their own panel.
- Keep the add-food form compact; in tablet mode it opens as an overlay rather than permanently consuming dashboard space.
- If a daily goal already exists, its progress may remain visible in tablet mode; keep the weekly summary and goal editor hidden there unless the user asks.
- Keep related form values together in compact rows and allow them to wrap on narrower screens.

## Exercise

- Exercise entries (Gym, Running, Bike) live in their own SQLite `ExerciseDbContext` (`ExerciseDbConnection`, `Migrations/ExerciseDb`) behind `IExerciseService` (`HomeServerPage.Client/Data/Exercise`; server `Data/Exercise`, `ExerciseController`), so the future exercise tracker can extend it.
- `ExerciseCalorieCalculator`: Gym 300 kcal; Running 250 kcal per 3 km; Bike 20 kcal/km up to 25 km (500), linear to 1000 kcal at 40 km, 25 kcal/km beyond. The server recomputes burned kcal and stores it as a snapshot.
- Consumed and burned kcal remain separate totals; net is consumed minus burned and may be negative. Exercise appears in the flat time-ordered daily log (add via the "Exercise" mode of the entry form; delete only), with daily consumed, burned, and net values. The daily goal bars use the goal as the scale, with consumed progress and an overlapping green burned segment; consumed calories above the goal get an additional red over-goal overlay. Of the green and red overlays, the shorter one renders on top so both extents remain visible. Net is displayed beside the bars, and the fridge widget shows net calories. History charts remain consumed-only.

## Implementation map

- `/calories` and `/tablet-dashboard/calories` use the shared page component `HomeServerPage/HomeServerPage.Client/Pages/Calories/Calories.razor`, with `CaloriesPage.razor` and `TabletCalories.razor` selecting the mode.
- Normal and tablet navigation are separate. Preserve both when changing routes or page availability.
- Client contracts and HTTP access are under `HomeServerPage/HomeServerPage.Client/Data/Calories`; server behavior and persistence are under `HomeServerPage/HomeServerPage/Data/Calories` and `Controllers/CaloriesController.cs`.
- The tracker has its own SQLite `CaloriesDbContext` and migrations. Follow the existing EF Core migration and service patterns for persistence changes.

Ask before changing calorie-log semantics, reusable-food behavior, totals, chart meaning, goal scope, or tablet priorities when the request does not specify the intended behavior. These established requirements are settled; do not ask the user to decide them again unless they request a change.
