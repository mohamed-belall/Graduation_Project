using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class editclinicIdname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_secretaries_clinics_clininId",
                table: "secretaries");

            migrationBuilder.RenameColumn(
                name: "clininId",
                table: "secretaries",
                newName: "clinicId");

            migrationBuilder.RenameIndex(
                name: "IX_secretaries_clininId",
                table: "secretaries",
                newName: "IX_secretaries_clinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_secretaries_clinics_clinicId",
                table: "secretaries",
                column: "clinicId",
                principalTable: "clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_secretaries_clinics_clinicId",
                table: "secretaries");

            migrationBuilder.RenameColumn(
                name: "clinicId",
                table: "secretaries",
                newName: "clininId");

            migrationBuilder.RenameIndex(
                name: "IX_secretaries_clinicId",
                table: "secretaries",
                newName: "IX_secretaries_clininId");

            migrationBuilder.AddForeignKey(
                name: "FK_secretaries_clinics_clininId",
                table: "secretaries",
                column: "clininId",
                principalTable: "clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
