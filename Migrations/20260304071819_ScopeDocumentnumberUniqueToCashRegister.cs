using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubTreasury.Migrations
{
    /// <inheritdoc />
    public partial class ScopeDocumentnumberUniqueToCashRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Documentnumber",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Documentnumber_CashRegisterId",
                table: "Transactions",
                columns: new[] { "Documentnumber", "CashRegisterId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Documentnumber_CashRegisterId",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Documentnumber",
                table: "Transactions",
                column: "Documentnumber",
                unique: true);
        }
    }
}
