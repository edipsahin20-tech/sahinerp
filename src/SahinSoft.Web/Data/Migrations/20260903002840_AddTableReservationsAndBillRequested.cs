using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTableReservationsAndBillRequested : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BillRequestedAtUtc",
                table: "RestaurantChecks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RestaurantTableReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantTableId = table.Column<int>(type: "int", nullable: false),
                    ReservedForUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTableReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantTableReservations_RestaurantTables_RestaurantTableId",
                        column: x => x.RestaurantTableId,
                        principalTable: "RestaurantTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableReservations_OneActivePerTable",
                table: "RestaurantTableReservations",
                column: "RestaurantTableId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableReservations_RecordId",
                table: "RestaurantTableReservations",
                column: "RecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantTableReservations");

            migrationBuilder.DropColumn(
                name: "BillRequestedAtUtc",
                table: "RestaurantChecks");
        }
    }
}
