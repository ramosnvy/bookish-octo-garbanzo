using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaIdToHistoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Historias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Historias_EmpresaId",
                table: "Historias",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Historias_Empresas_EmpresaId",
                table: "Historias",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Historias_Empresas_EmpresaId",
                table: "Historias");

            migrationBuilder.DropIndex(
                name: "IX_Historias_EmpresaId",
                table: "Historias");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Historias");
        }
    }
}
