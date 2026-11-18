using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations.LocationsDb
{
    /// <inheritdoc />
    public partial class InitialLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    ElevationMeters = table.Column<double>(type: "REAL", nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    LegacyObservationPointId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.CheckConstraint("CK_Locations_Latitude", "Latitude >= -90 AND Latitude <= 90");
                    table.CheckConstraint("CK_Locations_Longitude", "Longitude >= -180 AND Longitude <= 180");
                });

            migrationBuilder.CreateTable(
                name: "SeedStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InitialLocationsSeeded = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Selections",
                columns: table => new
                {
                    Feature = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsInitialized = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Selections", x => x.Feature);
                    table.ForeignKey(
                        name: "FK_Selections_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LegacyObservationPointId",
                table: "Locations",
                column: "LegacyObservationPointId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Selections_LocationId",
                table: "Selections",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeedStates");

            migrationBuilder.DropTable(
                name: "Selections");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
