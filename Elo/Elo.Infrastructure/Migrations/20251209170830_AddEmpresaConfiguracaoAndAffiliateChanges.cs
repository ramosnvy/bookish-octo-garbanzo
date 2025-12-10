using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaConfiguracaoAndAffiliateChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FornecedorId",
                table: "ContasPagar",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AfiliadoId",
                table: "ContasPagar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AfiliadoId",
                table: "Assinaturas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmpresaConfiguracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    JurosValor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    JurosTipo = table.Column<int>(type: "integer", nullable: false),
                    MoraValor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MoraTipo = table.Column<int>(type: "integer", nullable: false),
                    DiaPagamentoAfiliado = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaConfiguracoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaConfiguracoes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_AfiliadoId",
                table: "ContasPagar",
                column: "AfiliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_AfiliadoId",
                table: "Assinaturas",
                column: "AfiliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaConfiguracoes_EmpresaId",
                table: "EmpresaConfiguracoes",
                column: "EmpresaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assinaturas_Afiliados_AfiliadoId",
                table: "Assinaturas",
                column: "AfiliadoId",
                principalTable: "Afiliados",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ContasPagar_Afiliados_AfiliadoId",
                table: "ContasPagar",
                column: "AfiliadoId",
                principalTable: "Afiliados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinaturas_Afiliados_AfiliadoId",
                table: "Assinaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_ContasPagar_Afiliados_AfiliadoId",
                table: "ContasPagar");

            migrationBuilder.DropTable(
                name: "EmpresaConfiguracoes");

            migrationBuilder.DropIndex(
                name: "IX_ContasPagar_AfiliadoId",
                table: "ContasPagar");

            migrationBuilder.DropIndex(
                name: "IX_Assinaturas_AfiliadoId",
                table: "Assinaturas");

            migrationBuilder.DropColumn(
                name: "AfiliadoId",
                table: "ContasPagar");

            migrationBuilder.DropColumn(
                name: "AfiliadoId",
                table: "Assinaturas");

            migrationBuilder.AlterColumn<int>(
                name: "FornecedorId",
                table: "ContasPagar",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
