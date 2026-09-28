using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeRestaurantTableOptionalOnSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RestaurantTableId",
                table: "RestaurantTableSessions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "RestaurantTableSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Channel",
                table: "RestaurantTableSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Geriye dönük dolgu (backfill) - yukarıdaki AddColumn'lar TÜM mevcut satırlara
            // geçersiz bir değer (BranchId=0, Channel=0 - enumda hiç yok) yazıyor. 2026-09-29
            // mimari karar ÖNCESİ HER satırın (Masa/Self/Paket fark etmeksizin, Self/Paket o
            // zaman hâlâ gizli bir sanal RestaurantTable/RestaurantSection'a bağlıydı) gerçek
            // bir RestaurantTableId'si vardı - bu yüzden BranchId/Channel'ı o zincirden (Masa->
            // Bölüm->Şube, bölüm adından kanal) doğru şekilde geriye dönük türetebiliyoruz. Bu
            // adım AŞAĞIDAKİ FK eklenmeden ÖNCE çalışmalı, yoksa BranchId=0 (var olmayan şube)
            // FK ihlaline düşer.
            migrationBuilder.Sql(@"
                UPDATE rts
                SET rts.BranchId = rs.BranchId,
                    rts.Channel = CASE
                        WHEN rs.Name = N'Self Satış' THEN 2
                        WHEN rs.Name = N'Paket' THEN 3
                        ELSE 1
                    END
                FROM RestaurantTableSessions rts
                INNER JOIN RestaurantTables rt ON rt.Id = rts.RestaurantTableId
                INNER JOIN RestaurantSections rs ON rs.Id = rt.RestaurantSectionId;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessions_BranchId",
                table: "RestaurantTableSessions",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantTableSessions_Branches_BranchId",
                table: "RestaurantTableSessions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantTableSessions_Branches_BranchId",
                table: "RestaurantTableSessions");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantTableSessions_BranchId",
                table: "RestaurantTableSessions");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "RestaurantTableSessions");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "RestaurantTableSessions");

            migrationBuilder.AlterColumn<int>(
                name: "RestaurantTableId",
                table: "RestaurantTableSessions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
