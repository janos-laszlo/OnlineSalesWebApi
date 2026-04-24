using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleSales.Migrations
{
    /// <inheritdoc />
    public partial class DropObjectUploadsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `object_uploads`");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The object_uploads table belonged to the removed ObjectUploadTracking module.
            // It is not recreated on rollback.
        }
    }
}
