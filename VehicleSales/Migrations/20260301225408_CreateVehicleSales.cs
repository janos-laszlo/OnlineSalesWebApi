using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleSales.Migrations
{
    /// <inheritdoc />
    public partial class CreateVehicleSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vehicle_makes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_makes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleMakeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_models_vehicle_makes_VehicleMakeId",
                        column: x => x.VehicleMakeId,
                        principalTable: "vehicle_makes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vehicle_sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Description = table.Column<string>(type: "VARCHAR(5000)", nullable: false),
                    AmountInCents = table.Column<uint>(type: "int unsigned", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    County = table.Column<string>(type: "VARCHAR(30)", nullable: false),
                    Locality = table.Column<string>(type: "VARCHAR(30)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    VehicleModelId = table.Column<int>(type: "int", nullable: false),
                    MileageInKilometers = table.Column<uint>(type: "int unsigned", nullable: true),
                    HorsePower = table.Column<uint>(type: "int unsigned", nullable: true),
                    VehicleVersion = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BodyType = table.Column<int>(type: "int", nullable: true),
                    EngineVolumeInCm3 = table.Column<uint>(type: "int unsigned", nullable: true),
                    ExteriorColor = table.Column<string>(type: "VARCHAR(30)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InteriorColor = table.Column<string>(type: "VARCHAR(30)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FuelType = table.Column<int>(type: "int", nullable: true),
                    VehicleManufacturingYear = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    VehicleNumberOfDoors = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    VehicleCondition = table.Column<int>(type: "int", nullable: true),
                    GearboxType = table.Column<int>(type: "int", nullable: true),
                    SteeringWheelSide = table.Column<int>(type: "int", nullable: true),
                    DriveType = table.Column<int>(type: "int", nullable: true),
                    NumberOfSeats = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    EmissionStandard = table.Column<int>(type: "int", nullable: true),
                    HasServiceHistory = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    HasAccidentHistory = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Vin = table.Column<string>(type: "VARCHAR(17)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumberOfPreviousOwners = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    BatteryCapacityInKWh = table.Column<uint>(type: "int unsigned", nullable: true),
                    RangeInKilometers = table.Column<uint>(type: "int unsigned", nullable: true),
                    AverageFuelConsumptionInLitersPer100Km = table.Column<uint>(type: "int unsigned", nullable: true),
                    AverageBatteryConsumptionInKWhPer100Km = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    MassInKg = table.Column<uint>(type: "int unsigned", nullable: true),
                    MaximumLoadInKg = table.Column<uint>(type: "int unsigned", nullable: true),
                    Directory = table.Column<string>(type: "VARCHAR(32)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoKeys = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_sales_vehicle_models_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "vehicle_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_makes_Name",
                table: "vehicle_makes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_Name_VehicleMakeId",
                table: "vehicle_models",
                columns: new[] { "Name", "VehicleMakeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_VehicleMakeId",
                table: "vehicle_models",
                column: "VehicleMakeId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_sales_SellerId",
                table: "vehicle_sales",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_sales_VehicleModelId",
                table: "vehicle_sales",
                column: "VehicleModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_sales");

            migrationBuilder.DropTable(
                name: "vehicle_models");

            migrationBuilder.DropTable(
                name: "vehicle_makes");
        }
    }
}
