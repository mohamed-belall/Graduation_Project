using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class clinicsecretaryrelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_clincSecretaries_clinics_ClincId",
                table: "clincSecretaries");

            migrationBuilder.DropForeignKey(
                name: "FK_clincSecretaries_secretaries_SecretaryId",
                table: "clincSecretaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_clincSecretaries",
                table: "clincSecretaries");

            migrationBuilder.RenameTable(
                name: "clincSecretaries",
                newName: "ClincSecretary");

            migrationBuilder.RenameIndex(
                name: "IX_clincSecretaries_SecretaryId",
                table: "ClincSecretary",
                newName: "IX_ClincSecretary_SecretaryId");

            migrationBuilder.AddColumn<int>(
                name: "clininId",
                table: "secretaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClincSecretary",
                table: "ClincSecretary",
                columns: new[] { "ClincId", "SecretaryId" });

            migrationBuilder.CreateIndex(
                name: "IX_secretaries_clininId",
                table: "secretaries",
                column: "clininId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClincSecretary_clinics_ClincId",
                table: "ClincSecretary",
                column: "ClincId",
                principalTable: "clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ClincSecretary_secretaries_SecretaryId",
                table: "ClincSecretary",
                column: "SecretaryId",
                principalTable: "secretaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_secretaries_clinics_clininId",
                table: "secretaries",
                column: "clininId",
                principalTable: "clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClincSecretary_clinics_ClincId",
                table: "ClincSecretary");

            migrationBuilder.DropForeignKey(
                name: "FK_ClincSecretary_secretaries_SecretaryId",
                table: "ClincSecretary");

            migrationBuilder.DropForeignKey(
                name: "FK_secretaries_clinics_clininId",
                table: "secretaries");

            migrationBuilder.DropIndex(
                name: "IX_secretaries_clininId",
                table: "secretaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClincSecretary",
                table: "ClincSecretary");

            migrationBuilder.DropColumn(
                name: "clininId",
                table: "secretaries");

            migrationBuilder.RenameTable(
                name: "ClincSecretary",
                newName: "clincSecretaries");

            migrationBuilder.RenameIndex(
                name: "IX_ClincSecretary_SecretaryId",
                table: "clincSecretaries",
                newName: "IX_clincSecretaries_SecretaryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_clincSecretaries",
                table: "clincSecretaries",
                columns: new[] { "ClincId", "SecretaryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_clincSecretaries_clinics_ClincId",
                table: "clincSecretaries",
                column: "ClincId",
                principalTable: "clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_clincSecretaries_secretaries_SecretaryId",
                table: "clincSecretaries",
                column: "SecretaryId",
                principalTable: "secretaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
