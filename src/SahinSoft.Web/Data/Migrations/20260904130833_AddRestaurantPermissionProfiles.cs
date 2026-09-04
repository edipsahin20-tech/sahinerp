using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantPermissionProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestaurantPermissionProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CanCancelOrderLine = table.Column<bool>(type: "bit", nullable: false),
                    CanCancelReceipt = table.Column<bool>(type: "bit", nullable: false),
                    CanApplyDiscount = table.Column<bool>(type: "bit", nullable: false),
                    CanApplyComplimentary = table.Column<bool>(type: "bit", nullable: false),
                    CanEditKitchenSentLines = table.Column<bool>(type: "bit", nullable: false),
                    CanAddNote = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantPermissionProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantPersonnelPermissionProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PermissionProfileId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantPersonnelPermissionProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantPersonnelPermissionProfiles_RestaurantPermissionProfiles_PermissionProfileId",
                        column: x => x.PermissionProfileId,
                        principalTable: "RestaurantPermissionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPermissionProfiles_Name",
                table: "RestaurantPermissionProfiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPermissionProfiles_RecordId",
                table: "RestaurantPermissionProfiles",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPersonnelPermissionProfiles_PermissionProfileId",
                table: "RestaurantPersonnelPermissionProfiles",
                column: "PermissionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPersonnelPermissionProfiles_RecordId",
                table: "RestaurantPersonnelPermissionProfiles",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPersonnelPermissionProfiles_UserId_PermissionProfileId",
                table: "RestaurantPersonnelPermissionProfiles",
                columns: new[] { "UserId", "PermissionProfileId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantPersonnelPermissionProfiles");

            migrationBuilder.DropTable(
                name: "RestaurantPermissionProfiles");
        }
    }
}
