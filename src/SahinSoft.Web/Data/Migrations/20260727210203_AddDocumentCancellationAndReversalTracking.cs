using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentCancellationAndReversalTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "PaymentReceipts");

            migrationBuilder.AddColumn<int>(
                name: "ReversalOfId",
                table: "StockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "PaymentReceipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "PaymentReceipts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "PaymentReceipts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "PaymentReceipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByUserId",
                table: "PaymentReceipts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PaymentReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Invoices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Invoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByUserId",
                table: "Invoices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReversalOfId",
                table: "FinancialTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReversalOfId",
                table: "CurrentAccountTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ReversalOfId",
                table: "StockMovements",
                column: "ReversalOfId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ReversalOfId",
                table: "FinancialTransactions",
                column: "ReversalOfId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountTransactions_ReversalOfId",
                table: "CurrentAccountTransactions",
                column: "ReversalOfId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentAccountTransactions_CurrentAccountTransactions_ReversalOfId",
                table: "CurrentAccountTransactions",
                column: "ReversalOfId",
                principalTable: "CurrentAccountTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactions_ReversalOfId",
                table: "FinancialTransactions",
                column: "ReversalOfId",
                principalTable: "FinancialTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockMovements_ReversalOfId",
                table: "StockMovements",
                column: "ReversalOfId",
                principalTable: "StockMovements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentAccountTransactions_CurrentAccountTransactions_ReversalOfId",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_FinancialTransactions_ReversalOfId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockMovements_ReversalOfId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_ReversalOfId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_ReversalOfId",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CurrentAccountTransactions_ReversalOfId",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropColumn(
                name: "ReversalOfId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReversalOfId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "ReversalOfId",
                table: "CurrentAccountTransactions");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "PaymentReceipts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
