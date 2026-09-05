using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixRestaurantPaymentSubmissionKeyUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantPayments_SubmissionKey",
                table: "RestaurantPayments");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPayments_SubmissionKey",
                table: "RestaurantPayments",
                column: "SubmissionKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantPayments_SubmissionKey",
                table: "RestaurantPayments");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPayments_SubmissionKey",
                table: "RestaurantPayments",
                column: "SubmissionKey",
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");
        }
    }
}
