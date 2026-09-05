using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageOperationsCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedCourierId",
                table: "PackageOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "PackageOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformCommissionAmount",
                table: "PackageOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RestaurantCouriers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantCouriers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageOrders_AssignedCourierId",
                table: "PackageOrders",
                column: "AssignedCourierId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCouriers_RecordId",
                table: "RestaurantCouriers",
                column: "RecordId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PackageOrders_RestaurantCouriers_AssignedCourierId",
                table: "PackageOrders",
                column: "AssignedCourierId",
                principalTable: "RestaurantCouriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageOrders_RestaurantCouriers_AssignedCourierId",
                table: "PackageOrders");

            migrationBuilder.DropTable(
                name: "RestaurantCouriers");

            migrationBuilder.DropIndex(
                name: "IX_PackageOrders_AssignedCourierId",
                table: "PackageOrders");

            migrationBuilder.DropColumn(
                name: "AssignedCourierId",
                table: "PackageOrders");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "PackageOrders");

            migrationBuilder.DropColumn(
                name: "PlatformCommissionAmount",
                table: "PackageOrders");
        }
    }
}
