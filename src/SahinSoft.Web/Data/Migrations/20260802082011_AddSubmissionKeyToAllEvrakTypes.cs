using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionKeyToAllEvrakTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "StockTransfers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "StockSlips",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "PaymentReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "NegotiableInstruments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "NegotiableInstruments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "InventoryCounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Expenses",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "Expenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "DispatchNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "BusinessOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_CreatedByUserId_SubmissionKey",
                table: "StockTransfers",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockSlips_CreatedByUserId_SubmissionKey",
                table: "StockSlips",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_CreatedByUserId_SubmissionKey",
                table: "PaymentReceipts",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NegotiableInstruments_CreatedByUserId_SubmissionKey",
                table: "NegotiableInstruments",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_CreatedByUserId_SubmissionKey",
                table: "InventoryCounts",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedByUserId_SubmissionKey",
                table: "Expenses",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNotes_CreatedByUserId_SubmissionKey",
                table: "DispatchNotes",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrders_CreatedByUserId_SubmissionKey",
                table: "BusinessOrders",
                columns: new[] { "CreatedByUserId", "SubmissionKey" },
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockTransfers_CreatedByUserId_SubmissionKey",
                table: "StockTransfers");

            migrationBuilder.DropIndex(
                name: "IX_StockSlips_CreatedByUserId_SubmissionKey",
                table: "StockSlips");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_CreatedByUserId_SubmissionKey",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_NegotiableInstruments_CreatedByUserId_SubmissionKey",
                table: "NegotiableInstruments");

            migrationBuilder.DropIndex(
                name: "IX_InventoryCounts_CreatedByUserId_SubmissionKey",
                table: "InventoryCounts");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_CreatedByUserId_SubmissionKey",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_DispatchNotes_CreatedByUserId_SubmissionKey",
                table: "DispatchNotes");

            migrationBuilder.DropIndex(
                name: "IX_BusinessOrders_CreatedByUserId_SubmissionKey",
                table: "BusinessOrders");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "StockSlips");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "NegotiableInstruments");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "NegotiableInstruments");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "InventoryCounts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "DispatchNotes");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "BusinessOrders");
        }
    }
}
