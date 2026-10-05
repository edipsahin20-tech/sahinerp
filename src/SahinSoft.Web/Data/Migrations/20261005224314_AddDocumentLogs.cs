using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_CreatedAtUtc",
                table: "DocumentLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_EntityName_EntityId",
                table: "DocumentLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_RecordId",
                table: "DocumentLogs",
                column: "RecordId",
                unique: true);

            // Mevcut evraklar için "kayıt oluşturuldu" günlüğü (oluşturan kullanıcı ve tarih mevcut kayıttan alınır).
            foreach (var (entity, table, numberColumn) in new[] { ("Invoice", "Invoices", "InvoiceNumber"), ("PaymentReceipt", "PaymentReceipts", "ReceiptNumber"), ("DispatchNote", "DispatchNotes", "DispatchNumber"), ("BusinessOrder", "BusinessOrders", "OrderNumber"), ("Expense", "Expenses", "DocumentNumber") })
            {
                migrationBuilder.Sql($@"
INSERT INTO DocumentLogs (RecordId, CreatedAtUtc, EntityName, EntityId, DocumentNumber, Action, UserId, UserName, Details)
SELECT NEWID(), d.CreatedAtUtc, N'{entity}', d.Id, ISNULL(d.{numberColumn}, N''), N'Created', ISNULL(d.CreatedByUserId, N''), ISNULL(u.FullName, N'—'), N'Mevcut kayıttan'
FROM {table} d LEFT JOIN AspNetUsers u ON u.Id = d.CreatedByUserId;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentLogs");
        }
    }
}
