using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 1); // CustomerAccountType.Corporate — mevcut kayıtlarda 0 hiçbir enum değerine karşılık gelmez

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPerson",
                table: "Customers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerGroup",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultPaymentTermDays",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RiskLimit",
                table: "Customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AuthorizedPerson",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerGroup",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DefaultPaymentTermDays",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RiskLimit",
                table: "Customers");
        }
    }
}
