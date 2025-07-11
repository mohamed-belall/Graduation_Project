using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class makeclinicIdnullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_secretaries_clinics_clininId",
                table: "secretaries");

            migrationBuilder.DropIndex(
                name: "IX_secretaries_clininId",
                table: "secretaries");

            migrationBuilder.AlterColumn<int>(
                name: "clininId",
                table: "secretaries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_secretaries_clininId",
                table: "secretaries",
                column: "clininId",
                unique: true,
                filter: "[clininId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_secretaries_clinics_clininId",
                table: "secretaries",
                column: "clininId",
                principalTable: "clinics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_secretaries_clinics_clininId",
                table: "secretaries");

            migrationBuilder.DropIndex(
                name: "IX_secretaries_clininId",
                table: "secretaries");

            migrationBuilder.AlterColumn<int>(
                name: "clininId",
                table: "secretaries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_secretaries_clininId",
                table: "secretaries",
                column: "clininId",
                unique: true);

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
