using HomeServerPage.Client.Data.Locations;
using HomeServerPage.Data.Locations;
using HomeServerPage.Data.Weather;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HomeServerPage.Data.Locations;

public sealed class LocationSelectionService(
    IDbContextFactory<LocationsDbContext> dbContextFactory,
    LocationOperationCoordinator operationCoordinator,
    IWeatherLocationResolver weatherLocationResolver,
    IOptions<WeatherApiOptions> weatherOptions) : ILocationSelectionService
{
    public async Task<SavedLocation> GetSelectedLocationAsync(
        LocationFeature feature,
        CancellationToken cancellationToken = default)
    {
        ValidateFeature(feature);
        await operationCoordinator.WaitAsync(cancellationToken);

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var selection = await dbContext.Selections
                .SingleOrDefaultAsync(item => item.Feature == feature, cancellationToken)
                ?? throw new InvalidOperationException($"The {feature} location selection has not been initialized.");

            if (feature != LocationFeature.Weather || selection.IsInitialized)
            {
                return await GetSelectedLocationAsync(dbContext, selection, cancellationToken);
            }

            var configuredQuery = weatherOptions.Value.Location.Trim();
            if (string.IsNullOrWhiteSpace(configuredQuery)
                || string.Equals(configuredQuery, "Szczecin", StringComparison.OrdinalIgnoreCase))
            {
                selection.IsInitialized = true;
                await dbContext.SaveChangesAsync(cancellationToken);
                return await GetSelectedLocationAsync(dbContext, selection, cancellationToken);
            }

            var existingLocation = await dbContext.Locations
                .FirstOrDefaultAsync(
                    location => location.Name.ToLower() == configuredQuery.ToLower(),
                    cancellationToken);
            if (existingLocation is not null)
            {
                selection.LocationId = existingLocation.Id;
                selection.IsInitialized = true;
                await dbContext.SaveChangesAsync(cancellationToken);
                return existingLocation;
            }
        }
        finally
        {
            operationCoordinator.Release();
        }

        var resolvedLocation = await weatherLocationResolver.ResolveAsync(
            weatherOptions.Value.Location.Trim(),
            cancellationToken);
        try
        {
            LocationValidation.Validate(resolvedLocation);
        }
        catch (ArgumentException exception)
        {
            throw new WeatherApiException(
                (int)System.Net.HttpStatusCode.BadGateway,
                $"The weather provider returned invalid location data: {exception.Message}");
        }

        resolvedLocation.Name = resolvedLocation.Name.Trim();
        resolvedLocation.TimeZoneId = resolvedLocation.TimeZoneId.Trim();

        await operationCoordinator.WaitAsync(cancellationToken);
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var selection = await dbContext.Selections
                .SingleOrDefaultAsync(item => item.Feature == LocationFeature.Weather, cancellationToken)
                ?? throw new InvalidOperationException("The Weather location selection has not been initialized.");
            if (selection.IsInitialized)
            {
                return await GetSelectedLocationAsync(dbContext, selection, cancellationToken);
            }

            var existingLocation = await dbContext.Locations
                .FirstOrDefaultAsync(
                    location => location.Latitude == resolvedLocation.Latitude
                        && location.Longitude == resolvedLocation.Longitude
                        && location.TimeZoneId == resolvedLocation.TimeZoneId,
                    cancellationToken);
            if (existingLocation is null)
            {
                dbContext.Locations.Add(resolvedLocation);
                await dbContext.SaveChangesAsync(cancellationToken);
                existingLocation = resolvedLocation;
            }

            selection.LocationId = existingLocation.Id;
            selection.IsInitialized = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return existingLocation;
        }
        finally
        {
            operationCoordinator.Release();
        }
    }

    public async Task<SavedLocation> SetSelectedLocationAsync(
        LocationFeature feature,
        int locationId,
        CancellationToken cancellationToken = default)
    {
        ValidateFeature(feature);
        await operationCoordinator.WaitAsync(cancellationToken);

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var location = await dbContext.Locations
                .SingleOrDefaultAsync(item => item.Id == locationId, cancellationToken)
                ?? throw new KeyNotFoundException($"Location {locationId} was not found.");
            var selection = await dbContext.Selections
                .SingleOrDefaultAsync(item => item.Feature == feature, cancellationToken)
                ?? throw new InvalidOperationException($"The {feature} location selection has not been initialized.");

            selection.LocationId = location.Id;
            selection.IsInitialized = true;
            await dbContext.SaveChangesAsync(cancellationToken);

            return location;
        }
        finally
        {
            operationCoordinator.Release();
        }
    }

    private static void ValidateFeature(LocationFeature feature)
    {
        if (!Enum.IsDefined(feature))
        {
            throw new ArgumentOutOfRangeException(nameof(feature), feature, "Unknown location feature.");
        }
    }

    private static async Task<SavedLocation> GetSelectedLocationAsync(
        LocationsDbContext dbContext,
        LocationSelection selection,
        CancellationToken cancellationToken)
    {
        return await dbContext.Locations
            .AsNoTracking()
            .SingleOrDefaultAsync(location => location.Id == selection.LocationId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {selection.Feature} location selection points to a missing location.");
    }
}
