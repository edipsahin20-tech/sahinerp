using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingPaymentsAndQuickPaySetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireReceiptPromptAfterQuickPay",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RestaurantCheckPendingPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantCheckId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialAccountId = table.Column<int>(type: "int", nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantCheckPendingPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantCheckPendingPayments_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantCheckPendingPayments_RestaurantChecks_RestaurantCheckId",
                        column: x => x.RestaurantCheckId,
                        principalTable: "RestaurantChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequireReceiptPromptAfterQuickPay",
                value: false);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCheckPendingPayments_FinancialAccountId",
                table: "RestaurantCheckPendingPayments",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCheckPendingPayments_RecordId",
                table: "RestaurantCheckPendingPayments",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCheckPendingPayments_RestaurantCheckId",
                table: "RestaurantCheckPendingPayments",
                column: "RestaurantCheckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantCheckPendingPayments");

            migrationBuilder.DropColumn(
                name: "RequireReceiptPromptAfterQuickPay",
                table: "InventorySettings");
        }
    }
}
