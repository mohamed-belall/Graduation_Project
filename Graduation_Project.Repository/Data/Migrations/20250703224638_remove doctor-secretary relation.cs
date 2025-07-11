using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class removedoctorsecretaryrelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_secretaries_SecretaryId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_SecretaryId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SecretaryId",
                table: "Doctors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecretaryId",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SecretaryId",
                table: "Doctors",
                column: "SecretaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_secretaries_SecretaryId",
                table: "Doctors",
                column: "SecretaryId",
                principalTable: "secretaries",
                principalColumn: "Id");
        }
    }
}
