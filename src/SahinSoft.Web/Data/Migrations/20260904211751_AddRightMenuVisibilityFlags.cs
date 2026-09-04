using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRightMenuVisibilityFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanClearOrder",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeHeldReceipts",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeHoldReceipt",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeKeyboard",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeePriceCheck",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeProductList",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeReceiptList",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeSendToKitchen",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeTableTransfer",
                table: "RestaurantPermissionProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableClearOrderButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableComplimentaryReceiptButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableHeldReceiptsButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableHoldReceiptButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableKeyboardButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePriceCheckButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableProductListButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableReceiptListButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSendToKitchenButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTableTransferButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTicketNoteButton",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EnableClearOrderButton", "EnableComplimentaryReceiptButton", "EnableHeldReceiptsButton", "EnableHoldReceiptButton", "EnableKeyboardButton", "EnablePriceCheckButton", "EnableProductListButton", "EnableReceiptListButton", "EnableSendToKitchenButton", "EnableTableTransferButton", "EnableTicketNoteButton" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true, true });

            // Var olan yetki profillerinde bu yeni kolonlar AddColumn defaultValue:false ile
            // eklenir (yalnızca yeni oluşturulan profiller C# tarafındaki "= true" varsayılanını
            // alır) - bu satır olmadan mevcut tüm profillerde (ör. Test-Kısıtlı) sağ menü
            // butonları geçişten hemen sonra sessizce kaybolurdu. Var olan tüm profiller için
            // varsayılan davranışı (görünür) korumak amacıyla true'ya çekiyoruz.
            migrationBuilder.Sql(@"UPDATE RestaurantPermissionProfiles SET
                CanSeeTableTransfer = 1,
                CanSeeSendToKitchen = 1,
                CanSeePriceCheck = 1,
                CanSeeKeyboard = 1,
                CanSeeHoldReceipt = 1,
                CanSeeHeldReceipts = 1,
                CanSeeProductList = 1,
                CanSeeReceiptList = 1,
                CanClearOrder = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanClearOrder",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "CanSeeHeldReceipts",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "CanSeeHoldReceipt",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "CanSeeKeyboard",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "CanSeePriceCheck",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "CanSeeProductList",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "CanSeeReceiptList",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "CanSeeSendToKitchen",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "CanSeeTableTransfer",
                table: "RestaurantPermissionProfiles");

            migrationBuilder.DropColumn(
                name: "EnableClearOrderButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableComplimentaryReceiptButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableHeldReceiptsButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableHoldReceiptButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableKeyboardButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnablePriceCheckButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableProductListButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableReceiptListButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableSendToKitchenButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableTableTransferButton",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableTicketNoteButton",
                table: "InventorySettings");
        }
    }
}
