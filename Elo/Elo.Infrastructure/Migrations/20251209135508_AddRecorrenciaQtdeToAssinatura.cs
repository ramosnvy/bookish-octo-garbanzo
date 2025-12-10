using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddRecorrenciaQtdeToAssinatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AssinaturaId",
                table: "ContasReceber",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssinaturaId",
                table: "ContasPagar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecorrenciaQtde",
                table: "Assinaturas",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssinaturaId",
                table: "ContasReceber");

            migrationBuilder.DropColumn(
                name: "AssinaturaId",
                table: "ContasPagar");

            migrationBuilder.DropColumn(
                name: "RecorrenciaQtde",
                table: "Assinaturas");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);
        }
    }
}
