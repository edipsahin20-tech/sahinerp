using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationReasonAndQuickNoteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReasonPresets",
                table: "InventorySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuickNotePresets",
                table: "InventorySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCancellationReason",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CancellationReasonPresets", "QuickNotePresets", "RequireCancellationReason" },
                values: new object[] { null, null, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReasonPresets",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "QuickNotePresets",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RequireCancellationReason",
                table: "InventorySettings");
        }
    }
}
