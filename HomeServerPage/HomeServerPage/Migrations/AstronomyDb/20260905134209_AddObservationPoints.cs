using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations.AstronomyDb
{
    /// <inheritdoc />
    public partial class AddObservationPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObservationPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    ElevationMeters = table.Column<double>(type: "REAL", nullable: true),
                    HorizonNorth = table.Column<double>(type: "REAL", nullable: false),
                    HorizonNorthEast = table.Column<double>(type: "REAL", nullable: false),
                    HorizonEast = table.Column<double>(type: "REAL", nullable: false),
                    HorizonSouthEast = table.Column<double>(type: "REAL", nullable: false),
                    HorizonSouth = table.Column<double>(type: "REAL", nullable: false),
                    HorizonSouthWest = table.Column<double>(type: "REAL", nullable: false),
                    HorizonWest = table.Column<double>(type: "REAL", nullable: false),
                    HorizonNorthWest = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservationPoints", x => x.Id);
                    table.CheckConstraint("CK_ObservationPoints_HorizonAltitudes", "HorizonNorth >= 0 AND HorizonNorth <= 90 AND HorizonNorthEast >= 0 AND HorizonNorthEast <= 90 AND HorizonEast >= 0 AND HorizonEast <= 90 AND HorizonSouthEast >= 0 AND HorizonSouthEast <= 90 AND HorizonSouth >= 0 AND HorizonSouth <= 90 AND HorizonSouthWest >= 0 AND HorizonSouthWest <= 90 AND HorizonWest >= 0 AND HorizonWest <= 90 AND HorizonNorthWest >= 0 AND HorizonNorthWest <= 90");
                    table.CheckConstraint("CK_ObservationPoints_Latitude", "Latitude >= -90 AND Latitude <= 90");
                    table.CheckConstraint("CK_ObservationPoints_Longitude", "Longitude >= -180 AND Longitude <= 180");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObservationPoints_Name",
                table: "ObservationPoints",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObservationPoints");
        }
    }
}
