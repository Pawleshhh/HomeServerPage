using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HomeServerPage.Migrations.AstronomyDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Eyepieces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FocalLength = table.Column<double>(type: "REAL", nullable: false),
                    FieldOfView = table.Column<double>(type: "REAL", nullable: false),
                    BarellSize = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eyepieces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Multiplier = table.Column<double>(type: "REAL", nullable: false),
                    BarellSize = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ResolutionWidthPx = table.Column<int>(type: "INTEGER", nullable: false),
                    ResolutionHeightPx = table.Column<int>(type: "INTEGER", nullable: false),
                    PixelSizeUm = table.Column<double>(type: "REAL", nullable: false),
                    SensorWidthMm = table.Column<double>(type: "REAL", nullable: false),
                    SensorHeightMm = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Telescopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Aperture = table.Column<double>(type: "REAL", nullable: false),
                    FocalLength = table.Column<double>(type: "REAL", nullable: false),
                    ApertureSpeed = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Telescopes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Eyepieces",
                columns: new[] { "Id", "BarellSize", "FieldOfView", "FocalLength", "Name" },
                values: new object[,]
                {
                    { 1, 1, 56.0, 28.0, "Sky-Watcher 28mm LET 2\"" },
                    { 2, 0, 52.0, 10.0, "SUPER 10mm" },
                    { 3, 0, 52.0, 25.0, "SUPER 25mm" }
                });

            migrationBuilder.InsertData(
                table: "Lenses",
                columns: new[] { "Id", "BarellSize", "Multiplier", "Name" },
                values: new object[,]
                {
                    { 1, 0, 2.0, "Sky-Watcher 2x Barlow Lens" },
                    { 2, 1, 0.5, "DO-GSO 0.5x 2\"" }
                });

            migrationBuilder.InsertData(
                table: "Sensors",
                columns: new[] { "Id", "Name", "PixelSizeUm", "ResolutionHeightPx", "ResolutionWidthPx", "SensorHeightMm", "SensorWidthMm" },
                values: new object[,]
                {
                    { 1, "Canon EOS 1100D", 5.1900000000000004, 2848, 4272, 14.800000000000001, 22.199999999999999 },
                    { 2, "ZWO ASI678MC", 2.0, 2160, 3840, 4.3200000000000003, 7.6799999999999997 }
                });

            migrationBuilder.InsertData(
                table: "Telescopes",
                columns: new[] { "Id", "Aperture", "ApertureSpeed", "FocalLength", "Name", "Type" },
                values: new object[,]
                {
                    { 1, 127.0, 11.800000000000001, 1500.0, "Sky-Watcher BKMAK 127 OTAW", 3 },
                    { 2, 72.0, 5.7999999999999998, 420.0, "Sky-Watcher Evostar 72ED 72/420 F6", 0 },
                    { 3, 130.0, 6.9000000000000004, 900.0, "Sky-Watcher BK 1309 130/900", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Eyepieces");

            migrationBuilder.DropTable(
                name: "Lenses");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "Telescopes");
        }
    }
}
