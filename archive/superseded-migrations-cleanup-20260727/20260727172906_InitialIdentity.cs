using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations;

/// <summary>
/// SQL Server başlangıç şeması 00000000000000_CreateIdentitySchema içinde
/// doğrudan oluşturulduğu için sağlayıcı dönüşümü gerektirmeyen uyumluluk migration'ıdır.
/// Migration kimliği sonraki kurulumlarla geriye dönük uyumluluk için korunur.
/// </summary>
public partial class InitialIdentity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
