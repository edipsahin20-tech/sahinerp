using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrintTemplatePrinterAndPrintJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComplimentary",
                table: "RetailSaleLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PrintTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TemplateType = table.Column<int>(type: "int", nullable: false),
                    LayoutJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaperWidthMm = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintTemplates_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Printers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    ConnectionType = table.Column<int>(type: "int", nullable: false),
                    ConnectionAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AgentBaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaperWidth = table.Column<int>(type: "int", nullable: false),
                    AutoCut = table.Column<bool>(type: "bit", nullable: false),
                    CopyCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PrintTemplateId = table.Column<int>(type: "int", nullable: true),
                    KitchenStationId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Printers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Printers_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Printers_KitchenStations_KitchenStationId",
                        column: x => x.KitchenStationId,
                        principalTable: "KitchenStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Printers_PrintTemplates_PrintTemplateId",
                        column: x => x.PrintTemplateId,
                        principalTable: "PrintTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrintJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrinterId = table.Column<int>(type: "int", nullable: false),
                    PrintTemplateId = table.Column<int>(type: "int", nullable: true),
                    RenderedEscPosBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsTestPrint = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintJobs_PrintTemplates_PrintTemplateId",
                        column: x => x.PrintTemplateId,
                        principalTable: "PrintTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrintJobs_Printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "Printers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Printers_BranchId",
                table: "Printers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Printers_KitchenStationId",
                table: "Printers",
                column: "KitchenStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Printers_PrintTemplateId",
                table: "Printers",
                column: "PrintTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Printers_RecordId",
                table: "Printers",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_Pending",
                table: "PrintJobs",
                column: "ProcessedAtUtc",
                filter: "[ProcessedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_PrinterId",
                table: "PrintJobs",
                column: "PrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_PrintTemplateId",
                table: "PrintJobs",
                column: "PrintTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_RecordId",
                table: "PrintJobs",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrintTemplates_BranchId",
                table: "PrintTemplates",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintTemplates_RecordId",
                table: "PrintTemplates",
                column: "RecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrintJobs");

            migrationBuilder.DropTable(
                name: "Printers");

            migrationBuilder.DropTable(
                name: "PrintTemplates");

            migrationBuilder.DropColumn(
                name: "IsComplimentary",
                table: "RetailSaleLines");
        }
    }
}
