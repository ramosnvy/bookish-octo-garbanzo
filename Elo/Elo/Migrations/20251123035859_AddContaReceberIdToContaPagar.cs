using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddContaReceberIdToContaPagar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContasPagar_EmpresaId",
                table: "ContasPagar");

            migrationBuilder.AlterColumn<int>(
                name: "ServicoPagamentoTipo",
                table: "Pessoas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PrazoPagamentoDias",
                table: "Pessoas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ContaReceberId",
                table: "ContasPagar",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_EmpresaId_ContaReceberId",
                table: "ContasPagar",
                columns: new[] { "EmpresaId", "ContaReceberId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContasPagar_EmpresaId_ContaReceberId",
                table: "ContasPagar");

            migrationBuilder.DropColumn(
                name: "ContaReceberId",
                table: "ContasPagar");

            migrationBuilder.AlterColumn<int>(
                name: "ServicoPagamentoTipo",
                table: "Pessoas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "PrazoPagamentoDias",
                table: "Pessoas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_EmpresaId",
                table: "ContasPagar",
                column: "EmpresaId");
        }
    }
}
