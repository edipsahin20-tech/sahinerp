using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultCashAccountBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Varsayılan "Merkez Kasa" hesabı (Id=1) hiçbir şubeye bağlı değilse (ve ortak hesap değilse) Merkez Şube'ye bağlanır:
            // yeni kurulumda restoran terminali bu kasayı seçebilsin. Yönetici zaten bir şube/ortak ataması yaptıysa dokunulmaz.
            migrationBuilder.Sql("UPDATE [FinancialAccounts] SET [BranchId] = 1 WHERE [Id] = 1 AND [BranchId] IS NULL AND [IsShared] = 0 AND EXISTS (SELECT 1 FROM [Branches] WHERE [Id] = 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma gerekmez (şube ataması zararsızdır).
        }
    }
}
