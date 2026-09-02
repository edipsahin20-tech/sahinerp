using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CategoryAndProductVisibilityRestrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DiscountNotApplicable",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PromotionNotApplicable",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VisibleInBranches",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "ProductCategories",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#6c757d");

            migrationBuilder.AddColumn<bool>(
                name: "DiscountNotApplicable",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ProductCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryId",
                table: "ProductCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PromotionNotApplicable",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // NOT: entity'deki "= true" varsayılanı sadece YENİ C# nesneleri için geçerli,
            // AddColumn'un defaultValue'sunu OTOMATİK etkilemiyor (EF Core'un bool için scaffold
            // ettiği varsayılan her zaman false) - bu 4 sütun burada elle true'ya çekildi, aksi
            // halde "Güncelle" ile mevcut veri korunan bir kurulumda önceden var olan kategoriler
            // sessizce Kısayol/Mobil/Online/Şube görünürlüğünü kaybederdi.
            migrationBuilder.AddColumn<bool>(
                name: "ShowAsShortcut",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInMobile",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInOnlineOrder",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "VisibleInBranches",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentCategoryId",
                table: "ProductCategories",
                column: "ParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_ProductCategories_ParentCategoryId",
                table: "ProductCategories",
                column: "ParentCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_ProductCategories_ParentCategoryId",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_ParentCategoryId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DiscountNotApplicable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PromotionNotApplicable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VisibleInBranches",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DiscountNotApplicable",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "PromotionNotApplicable",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ShowAsShortcut",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ShowInMobile",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ShowInOnlineOrder",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "VisibleInBranches",
                table: "ProductCategories");
        }
    }
}
