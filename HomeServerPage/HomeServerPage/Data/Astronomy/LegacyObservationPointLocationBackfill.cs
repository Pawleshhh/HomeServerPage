using System.Data;
using System.Data.Common;
using HomeServerPage.Data.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace HomeServerPage.Data.Astronomy;

public sealed class LegacyObservationPointLocationBackfill(
    IDbContextFactory<AstronomyDbContext> astronomyContextFactory,
    IDbContextFactory<LocationsDbContext> locationsContextFactory)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var astronomyContext =
            await astronomyContextFactory.CreateDbContextAsync(cancellationToken);
        var appliedMigrations = await astronomyContext.Database
            .GetAppliedMigrationsAsync(cancellationToken);
        if (appliedMigrations.Contains(ObservationPointLocationMigrationIds.RemoveLegacyCoordinates))
        {
            return;
        }

        var legacyPoints = await ReadLegacyPointsAsync(astronomyContext.Database.GetDbConnection(), cancellationToken);
        if (legacyPoints.Count == 0)
        {
            return;
        }

        foreach (var legacyPoint in legacyPoints)
        {
            var existingLocationId = await GetExistingLocationIdAsync(
                legacyPoint.Id,
                cancellationToken);
            var locationId = existingLocationId
                ?? await CreateImportedLocationAsync(legacyPoint, cancellationToken);

            await UpdateObservationPointLocationAsync(
                astronomyContext.Database.GetDbConnection(),
                legacyPoint.Id,
                locationId,
                cancellationToken);
        }

        var remainingPointCount = await CountUnlinkedPointsAsync(
            astronomyContext.Database.GetDbConnection(),
            cancellationToken);
        if (remainingPointCount != 0)
        {
            throw new InvalidOperationException(
                $"The Observation Point migration left {remainingPointCount} points without a Location.");
        }
    }

    private async Task<int?> GetExistingLocationIdAsync(
        int observationPointId,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await locationsContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Locations
            .AsNoTracking()
            .Where(location => location.LegacyObservationPointId == observationPointId)
            .Select(location => (int?)location.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<int> CreateImportedLocationAsync(
        LegacyObservationPoint legacyPoint,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await locationsContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingLocation = await dbContext.Locations
            .SingleOrDefaultAsync(
                location => location.LegacyObservationPointId == legacyPoint.Id,
                cancellationToken);
        if (existingLocation is not null)
        {
            return existingLocation.Id;
        }

        var location = new SavedLocation
        {
            Name = legacyPoint.Name,
            Latitude = legacyPoint.Latitude,
            Longitude = legacyPoint.Longitude,
            ElevationMeters = legacyPoint.ElevationMeters,
            TimeZoneId = "Europe/Warsaw",
            LegacyObservationPointId = legacyPoint.Id
        };
        LocationValidation.Validate(location);
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return location.Id;
    }

    private static async Task<List<LegacyObservationPoint>> ReadLegacyPointsAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        var openedConnection = connection.State != ConnectionState.Open;
        if (openedConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT Id, Name, Latitude, Longitude, ElevationMeters
                FROM ObservationPoints
                WHERE LocationId IS NULL
                ORDER BY Id
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var points = new List<LegacyObservationPoint>();
            while (await reader.ReadAsync(cancellationToken))
            {
                points.Add(new LegacyObservationPoint(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDouble(2),
                    reader.GetDouble(3),
                    reader.IsDBNull(4) ? 0 : reader.GetDouble(4)));
            }

            return points;
        }
        finally
        {
            if (openedConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task UpdateObservationPointLocationAsync(
        DbConnection connection,
        int observationPointId,
        int locationId,
        CancellationToken cancellationToken)
    {
        var openedConnection = connection.State != ConnectionState.Open;
        if (openedConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE ObservationPoints
                SET LocationId = @locationId
                WHERE Id = @observationPointId AND LocationId IS NULL
                """;
            AddParameter(command, "@locationId", locationId);
            AddParameter(command, "@observationPointId", observationPointId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (openedConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<int> CountUnlinkedPointsAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        var openedConnection = connection.State != ConnectionState.Open;
        if (openedConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM ObservationPoints WHERE LocationId IS NULL";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
        }
        finally
        {
            if (openedConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static void AddParameter(DbCommand command, string name, int value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private sealed record LegacyObservationPoint(
        int Id,
        string Name,
        double Latitude,
        double Longitude,
        double ElevationMeters);
}
