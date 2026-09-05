using HomeServerPage.Client.Data.Astronomy.Telescopes;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace HomeServerPage.Data.Astronomy;

public class AstronomyDbContext(DbContextOptions<AstronomyDbContext> options) : DbContext(options)
{
    public DbSet<TelescopeItem> Telescopes { get; set; }

    public DbSet<TelescopeEyepiece> Eyepieces { get; set; }

    public DbSet<TelescopeLens> Lenses { get; set; }

    public DbSet<SensorItem> Sensors { get; set; }

    public DbSet<DeepSkyObject> DeepSkyObjects { get; set; }

    public DbSet<ObservationPoint> ObservationPoints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        AddTelescopes(modelBuilder);
        AddEyepieces(modelBuilder);
        AddLenses(modelBuilder);
        AddSensors(modelBuilder);
        AddDeepSkyObjects(modelBuilder);
        AddObservationPoints(modelBuilder);
    }

    private void AddTelescopes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelescopeItem>().HasData(
            new TelescopeItem(
                Name: "Sky-Watcher BKMAK 127 OTAW",
                Type: TelescopeType.MaksutovCassegrain,
                Aperture: 127,
                FocalLength: 1500,
                ApertureSpeed: 11.8)
            {
                Id = 1
            },
            new TelescopeItem(
                Name: "Sky-Watcher Evostar 72ED 72/420 F6",
                Type: TelescopeType.Refractor,
                Aperture: 72,
                FocalLength: 420,
                ApertureSpeed: 5.8)
            {
                Id = 2
            },
            new TelescopeItem(
                Name: "Sky-Watcher BK 1309 130/900",
                Type: TelescopeType.Newtonian,
                Aperture: 130,
                FocalLength: 900,
                ApertureSpeed: 6.9)
            {
                Id = 3
            });

    }

    private void AddEyepieces(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelescopeEyepiece>().HasData(
            new TelescopeEyepiece(
                Name: "Sky-Watcher 28mm LET 2\"",
                FocalLength: 28,
                FieldOfView: 56,
                BarellSize: BarellSize.Size2Inches)
            {
                Id = 1
            },
            new TelescopeEyepiece(
                Name: "SUPER 10mm",
                FocalLength: 10,
                FieldOfView: 52,
                BarellSize: BarellSize.Size125Inches)
            {
                Id = 2
            },
            new TelescopeEyepiece(
                Name: "SUPER 25mm",
                FocalLength: 25,
                FieldOfView: 52,
                BarellSize: BarellSize.Size125Inches)
            {
                Id = 3
            });
    }

    private void AddLenses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelescopeLens>().HasData(
            new TelescopeLens(
                Name: "Sky-Watcher 2x Barlow Lens",
                Multiplier: 2,
                BarellSize: BarellSize.Size125Inches)
            {
                Id = 1
            },
            new TelescopeLens(
                Name: "DO-GSO 0.5x 2\"",
                Multiplier: 0.5,
                BarellSize: BarellSize.Size2Inches)
            {
                Id = 2
            });
    }

    private void AddSensors(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorItem>().HasData(
            new SensorItem(
                Name: "Canon EOS 1100D",
                ResolutionWidthPx: 4272,
                ResolutionHeightPx: 2848,
                PixelSizeUm: 5.19,
                SensorWidthMm: 22.2,
                SensorHeightMm: 14.8)
            {
                Id = 1
            },
            new SensorItem(
                Name: "ZWO ASI678MC",
                ResolutionWidthPx: 3840,
                ResolutionHeightPx: 2160,
                PixelSizeUm: 2,
                SensorWidthMm: 7.68,
                SensorHeightMm: 4.32)
            {
                Id = 2
            });
    }

    private void AddDeepSkyObjects(ModelBuilder modelBuilder)
    {
        using var stream = typeof(AstronomyDbContext).Assembly.GetManifestResourceStream(
            "HomeServerPage.wwwroot.data.deepSkyObjects.json");

        if (stream is null)
        {
            return;
        }

        using var reader = new StreamReader(stream);
        using var document = JsonDocument.Parse(reader.ReadToEnd());

        var deepSkyObjects = document.RootElement
            .EnumerateArray()
            .Select((element, index) =>
            {
                var sizeParts = element.GetProperty("Size").GetString()!.Split('x');
                var width = double.Parse(sizeParts[0], CultureInfo.InvariantCulture);
                var height = sizeParts.Length == 1
                    ? width
                    : double.Parse(sizeParts[1], CultureInfo.InvariantCulture);

                return new DeepSkyObject(
                    Symbol: element.GetProperty("Symbol").GetString()!,
                    Catalog: element.GetProperty("Catalog").GetString()!,
                    Width: width,
                    Height: height)
                {
                    Id = index + 1
                };
            });

        modelBuilder.Entity<DeepSkyObject>().HasData(deepSkyObjects);
    }

    private void AddObservationPoints(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ObservationPoint>();

        entity.Property(point => point.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.HasIndex(point => point.Name);

        entity.ToTable(table => table.HasCheckConstraint(
            "CK_ObservationPoints_Latitude",
            "Latitude >= -90 AND Latitude <= 90"));

        entity.ToTable(table => table.HasCheckConstraint(
            "CK_ObservationPoints_Longitude",
            "Longitude >= -180 AND Longitude <= 180"));

        entity.ToTable(table => table.HasCheckConstraint(
            "CK_ObservationPoints_HorizonAltitudes",
            "HorizonNorth >= 0 AND HorizonNorth <= 90 " +
            "AND HorizonNorthEast >= 0 AND HorizonNorthEast <= 90 " +
            "AND HorizonEast >= 0 AND HorizonEast <= 90 " +
            "AND HorizonSouthEast >= 0 AND HorizonSouthEast <= 90 " +
            "AND HorizonSouth >= 0 AND HorizonSouth <= 90 " +
            "AND HorizonSouthWest >= 0 AND HorizonSouthWest <= 90 " +
            "AND HorizonWest >= 0 AND HorizonWest <= 90 " +
            "AND HorizonNorthWest >= 0 AND HorizonNorthWest <= 90"));
    }
}
