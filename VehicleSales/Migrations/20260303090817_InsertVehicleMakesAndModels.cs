using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleSales.Migrations
{
    /// <inheritdoc />
    public partial class InsertVehicleMakesAndModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                File.ReadAllText(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "Data",
                        "insert vehicle makes and models.sql")));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM {Tables.VehicleMakes};");
        }
    }
}
