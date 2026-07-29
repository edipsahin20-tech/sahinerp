using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteAmountDiscountAndPriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity",
                table: "SalesPriceListItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Quotes_Totals",
                table: "Quotes");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountDiscount",
                table: "Quotes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity",
                table: "SalesPriceListItems",
                columns: new[] { "SalesPriceListId", "ProductId", "ProductVariantId", "MinimumQuantity" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quotes_Totals",
                table: "Quotes",
                sql: "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [AmountDiscount] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity",
                table: "SalesPriceListItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Quotes_Totals",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "AmountDiscount",
                table: "Quotes");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity",
                table: "SalesPriceListItems",
                columns: new[] { "SalesPriceListId", "ProductId", "ProductVariantId", "MinimumQuantity" },
                unique: true,
                filter: "[ProductVariantId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quotes_Totals",
                table: "Quotes",
                sql: "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0");
        }
    }
}
