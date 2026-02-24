using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubTreasury.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalYearStartMonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FiscalYearStartMonth",
                table: "CashRegisters",
                type: "integer",
                nullable: false,
                defaultValue: 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiscalYearStartMonth",
                table: "CashRegisters");
        }
    }
}
