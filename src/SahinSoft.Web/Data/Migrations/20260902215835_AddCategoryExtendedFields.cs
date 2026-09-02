using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternateName",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentPurchase",
                table: "ProductCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentSale",
                table: "ProductCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPoints",
                table: "ProductCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "LoyaltyPointsPercent",
                table: "ProductCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInReceiptImage",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternateName",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DiscountPercentPurchase",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DiscountPercentSale",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "LoyaltyPoints",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "LoyaltyPointsPercent",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ShowInReceiptImage",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ProductCategories");
        }
    }
}
