using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNegotiableInstrumentAccountingLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndorsedAtUtc",
                table: "NegotiableInstruments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EndorsedToCustomerId",
                table: "NegotiableInstruments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAtUtc",
                table: "NegotiableInstruments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SettlementFinancialAccountId",
                table: "NegotiableInstruments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NegotiableInstrumentId",
                table: "FinancialTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NegotiableInstrumentId",
                table: "CurrentAccountTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NegotiableInstruments_EndorsedToCustomerId",
                table: "NegotiableInstruments",
                column: "EndorsedToCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_NegotiableInstruments_SettlementFinancialAccountId",
                table: "NegotiableInstruments",
                column: "SettlementFinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_NegotiableInstrumentId",
                table: "FinancialTransactions",
                column: "NegotiableInstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountTransactions_NegotiableInstrumentId",
                table: "CurrentAccountTransactions",
                column: "NegotiableInstrumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentAccountTransactions_NegotiableInstruments_NegotiableInstrumentId",
                table: "CurrentAccountTransactions",
                column: "NegotiableInstrumentId",
                principalTable: "NegotiableInstruments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_NegotiableInstruments_NegotiableInstrumentId",
                table: "FinancialTransactions",
                column: "NegotiableInstrumentId",
                principalTable: "NegotiableInstruments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_NegotiableInstruments_Customers_EndorsedToCustomerId",
                table: "NegotiableInstruments",
                column: "EndorsedToCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NegotiableInstruments_FinancialAccounts_SettlementFinancialAccountId",
                table: "NegotiableInstruments",
                column: "SettlementFinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentAccountTransactions_NegotiableInstruments_NegotiableInstrumentId",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_NegotiableInstruments_NegotiableInstrumentId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_NegotiableInstruments_Customers_EndorsedToCustomerId",
                table: "NegotiableInstruments");

            migrationBuilder.DropForeignKey(
                name: "FK_NegotiableInstruments_FinancialAccounts_SettlementFinancialAccountId",
                table: "NegotiableInstruments");

            migrationBuilder.DropIndex(
                name: "IX_NegotiableInstruments_EndorsedToCustomerId",
                table: "NegotiableInstruments");

            migrationBuilder.DropIndex(
                name: "IX_NegotiableInstruments_SettlementFinancialAccountId",
                table: "NegotiableInstruments");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_NegotiableInstrumentId",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CurrentAccountTransactions_NegotiableInstrumentId",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropColumn(
                name: "EndorsedAtUtc",
                table: "NegotiableInstruments");

            migrationBuilder.DropColumn(
                name: "EndorsedToCustomerId",
                table: "NegotiableInstruments");

            migrationBuilder.DropColumn(
                name: "SettledAtUtc",
                table: "NegotiableInstruments");

            migrationBuilder.DropColumn(
                name: "SettlementFinancialAccountId",
                table: "NegotiableInstruments");

            migrationBuilder.DropColumn(
                name: "NegotiableInstrumentId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "NegotiableInstrumentId",
                table: "CurrentAccountTransactions");
        }
    }
}
