using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyDefaultBranchWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultBranchId",
                table: "CompanySettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultWarehouseId",
                table: "CompanySettings",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DefaultBranchId", "DefaultWarehouseId" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_DefaultBranchId",
                table: "CompanySettings",
                column: "DefaultBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_DefaultWarehouseId",
                table: "CompanySettings",
                column: "DefaultWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySettings_Branches_DefaultBranchId",
                table: "CompanySettings",
                column: "DefaultBranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySettings_Warehouses_DefaultWarehouseId",
                table: "CompanySettings",
                column: "DefaultWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanySettings_Branches_DefaultBranchId",
                table: "CompanySettings");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanySettings_Warehouses_DefaultWarehouseId",
                table: "CompanySettings");

            migrationBuilder.DropIndex(
                name: "IX_CompanySettings_DefaultBranchId",
                table: "CompanySettings");

            migrationBuilder.DropIndex(
                name: "IX_CompanySettings_DefaultWarehouseId",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "DefaultBranchId",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "DefaultWarehouseId",
                table: "CompanySettings");
        }
    }
}
