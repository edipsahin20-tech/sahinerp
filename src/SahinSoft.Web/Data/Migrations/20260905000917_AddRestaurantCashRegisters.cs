using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantCashRegisters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestaurantCashRegisters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CashFinancialAccountId = table.Column<int>(type: "int", nullable: false),
                    CreditCardFinancialAccountId = table.Column<int>(type: "int", nullable: false),
                    MealCardFinancialAccountId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantCashRegisters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantCashRegisters_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantCashRegisters_FinancialAccounts_CashFinancialAccountId",
                        column: x => x.CashFinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantCashRegisters_FinancialAccounts_CreditCardFinancialAccountId",
                        column: x => x.CreditCardFinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantCashRegisters_FinancialAccounts_MealCardFinancialAccountId",
                        column: x => x.MealCardFinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RestaurantCashRegisters",
                columns: new[] { "Id", "BranchId", "CashFinancialAccountId", "CreatedAtUtc", "CreditCardFinancialAccountId", "IsActive", "MealCardFinancialAccountId", "Name", "Note", "UpdatedAtUtc" },
                values: new object[] { 1, 1, 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, 1, "Ana Kasa", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashRegisters_BranchId",
                table: "RestaurantCashRegisters",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashRegisters_CashFinancialAccountId",
                table: "RestaurantCashRegisters",
                column: "CashFinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashRegisters_CreditCardFinancialAccountId",
                table: "RestaurantCashRegisters",
                column: "CreditCardFinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashRegisters_MealCardFinancialAccountId",
                table: "RestaurantCashRegisters",
                column: "MealCardFinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashRegisters_RecordId",
                table: "RestaurantCashRegisters",
                column: "RecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantCashRegisters");
        }
    }
}
