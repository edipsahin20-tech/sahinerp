using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionalProductVariantPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "InvoiceLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireProductVariant",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequireProductVariant",
                value: false);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ProductVariantId",
                table: "InvoiceLines",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_ProductVariants_ProductVariantId",
                table: "InvoiceLines",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_ProductVariants_ProductVariantId",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_ProductVariantId",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "RequireProductVariant",
                table: "InventorySettings");
        }
    }
}
