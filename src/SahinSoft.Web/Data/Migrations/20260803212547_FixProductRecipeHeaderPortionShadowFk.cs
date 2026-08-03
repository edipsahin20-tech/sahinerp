using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixProductRecipeHeaderPortionShadowFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecipeHeaders_ProductPortions_ProductPortionId1",
                table: "ProductRecipeHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ProductRecipeHeaders_ProductPortionId1",
                table: "ProductRecipeHeaders");

            migrationBuilder.DropColumn(
                name: "ProductPortionId1",
                table: "ProductRecipeHeaders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductPortionId1",
                table: "ProductRecipeHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeHeaders_ProductPortionId1",
                table: "ProductRecipeHeaders",
                column: "ProductPortionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecipeHeaders_ProductPortions_ProductPortionId1",
                table: "ProductRecipeHeaders",
                column: "ProductPortionId1",
                principalTable: "ProductPortions",
                principalColumn: "Id");
        }
    }
}
