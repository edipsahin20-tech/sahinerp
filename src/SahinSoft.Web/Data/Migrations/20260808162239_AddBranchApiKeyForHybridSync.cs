using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchApiKeyForHybridSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Branches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApiKey",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ApiKey",
                table: "Branches",
                column: "ApiKey",
                unique: true,
                filter: "[ApiKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Branches_ApiKey",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Branches");
        }
    }
}
