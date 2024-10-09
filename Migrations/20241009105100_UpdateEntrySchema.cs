using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntrySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_BusinessSectors_BusinessSectorId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_BusinessSectorId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "BusinessSectorId",
                table: "Entries");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_SectorId",
                table: "Entries",
                column: "SectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_BusinessSectors_SectorId",
                table: "Entries",
                column: "SectorId",
                principalTable: "BusinessSectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_BusinessSectors_SectorId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_SectorId",
                table: "Entries");

            migrationBuilder.AddColumn<int>(
                name: "BusinessSectorId",
                table: "Entries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_BusinessSectorId",
                table: "Entries",
                column: "BusinessSectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_BusinessSectors_BusinessSectorId",
                table: "Entries",
                column: "BusinessSectorId",
                principalTable: "BusinessSectors",
                principalColumn: "Id");
        }
    }
}
