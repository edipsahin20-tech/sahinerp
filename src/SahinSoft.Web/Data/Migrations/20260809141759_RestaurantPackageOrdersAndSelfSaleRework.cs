using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestaurantPackageOrdersAndSelfSaleRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReadyAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispatchedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RestaurantCheckId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageOrders_RestaurantChecks_RestaurantCheckId",
                        column: x => x.RestaurantCheckId,
                        principalTable: "RestaurantChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageOrders_PackageNumber",
                table: "PackageOrders",
                column: "PackageNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageOrders_RecordId",
                table: "PackageOrders",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageOrders_RestaurantCheckId",
                table: "PackageOrders",
                column: "RestaurantCheckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageOrders_SubmissionKey",
                table: "PackageOrders",
                column: "SubmissionKey",
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            // RESTAURANT_CHECK sayacındaki desenin aynısı (bkz. RestaurantModulePhase2Prereqs) -
            // Id'yi IDENTITY'den almasına izin verilir, IF NOT EXISTS ile idempotent.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [NumberSequences] WHERE [Key] = N'PACKAGE_ORDER')
BEGIN
    INSERT INTO [NumberSequences] ([Key], [Prefix], [NextNumber], [Padding], [RecordId], [CreatedAtUtc])
    VALUES (N'PACKAGE_ORDER', N'PK-', 1, 5, NEWID(), SYSUTCDATETIME());
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM [NumberSequences] WHERE [Key] = N'PACKAGE_ORDER';");

            migrationBuilder.DropTable(
                name: "PackageOrders");
        }
    }
}
