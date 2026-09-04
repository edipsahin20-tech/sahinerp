using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondApprovalAndPermissionAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireSecondApprovalForAddNote",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireSecondApprovalForCancelOrderLine",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireSecondApprovalForCancelReceipt",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireSecondApprovalForComplimentary",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireSecondApprovalForDiscount",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireSecondApprovalForEditKitchenSentLines",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RestaurantPermissionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApproverUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RestaurantCheckId = table.Column<int>(type: "int", nullable: true),
                    RestaurantOrderLineId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantPermissionAuditLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RequireSecondApprovalForAddNote", "RequireSecondApprovalForCancelOrderLine", "RequireSecondApprovalForCancelReceipt", "RequireSecondApprovalForComplimentary", "RequireSecondApprovalForDiscount", "RequireSecondApprovalForEditKitchenSentLines" },
                values: new object[] { false, false, false, false, false, false });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPermissionAuditLogs_CreatedAtUtc",
                table: "RestaurantPermissionAuditLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPermissionAuditLogs_RecordId",
                table: "RestaurantPermissionAuditLogs",
                column: "RecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantPermissionAuditLogs");

            migrationBuilder.DropColumn(
                name: "RequireSecondApprovalForAddNote",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RequireSecondApprovalForCancelOrderLine",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RequireSecondApprovalForCancelReceipt",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RequireSecondApprovalForComplimentary",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RequireSecondApprovalForDiscount",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RequireSecondApprovalForEditKitchenSentLines",
                table: "InventorySettings");
        }
    }
}
