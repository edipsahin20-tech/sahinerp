using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketDiscountAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RestaurantChecks_Amounts",
                table: "RestaurantChecks");

            migrationBuilder.AddColumn<decimal>(
                name: "TicketDiscountAmount",
                table: "RestaurantChecks",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RestaurantChecks_Amounts",
                table: "RestaurantChecks",
                sql: "[SubtotalAmount] >= 0 AND [DiscountAmount] >= 0 AND [ServiceChargeAmount] >= 0 AND [TaxAmount] >= 0 AND [GrandTotal] >= 0 AND [TicketDiscountAmount] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RestaurantChecks_Amounts",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "TicketDiscountAmount",
                table: "RestaurantChecks");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RestaurantChecks_Amounts",
                table: "RestaurantChecks",
                sql: "[SubtotalAmount] >= 0 AND [DiscountAmount] >= 0 AND [ServiceChargeAmount] >= 0 AND [TaxAmount] >= 0 AND [GrandTotal] >= 0");
        }
    }
}
