using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations.AstronomyDb
{
    /// <inheritdoc />
    public partial class UseLocationForObservationPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ObservationPoints_Latitude",
                table: "ObservationPoints");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ObservationPoints_Longitude",
                table: "ObservationPoints");

            migrationBuilder.DropColumn(
                name: "ElevationMeters",
                table: "ObservationPoints");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ObservationPoints");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ObservationPoints");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ObservationPoints_LocationId",
                table: "ObservationPoints",
                sql: "LocationId IS NOT NULL AND LocationId > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ObservationPoints_LocationId",
                table: "ObservationPoints");

            migrationBuilder.AddColumn<double>(
                name: "ElevationMeters",
                table: "ObservationPoints",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ObservationPoints",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ObservationPoints",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ObservationPoints_Latitude",
                table: "ObservationPoints",
                sql: "Latitude >= -90 AND Latitude <= 90");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ObservationPoints_Longitude",
                table: "ObservationPoints",
                sql: "Longitude >= -180 AND Longitude <= 180");
        }
    }
}
