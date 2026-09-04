using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCollectionCariFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCollectionCari",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCollectionCari",
                table: "Customers");
        }
    }
}
