using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestoranModuluVeKdvSelect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KitchenPrinterName",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPoints",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowAsShortcut",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInMobile",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInOnlineOrder",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRestaurantModuleEnabled",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsRestaurantModuleEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "KitchenPrinterName", "LoyaltyPoints", "ShowAsShortcut", "ShowInMobile", "ShowInOnlineOrder" },
                values: new object[] { null, 0, true, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KitchenPrinterName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LoyaltyPoints",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShowAsShortcut",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShowInMobile",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShowInOnlineOrder",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsRestaurantModuleEnabled",
                table: "InventorySettings");
        }
    }
}
