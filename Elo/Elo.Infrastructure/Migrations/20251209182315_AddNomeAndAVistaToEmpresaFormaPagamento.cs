using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddNomeAndAVistaToEmpresaFormaPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AVista",
                table: "EmpresaFormasPagamento",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "EmpresaFormasPagamento",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AVista",
                table: "EmpresaFormasPagamento");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "EmpresaFormasPagamento");
        }
    }
}
