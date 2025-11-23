using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pessoas_Documento",
                table: "Pessoas");

            migrationBuilder.DropIndex(
                name: "IX_Pessoas_Email",
                table: "Pessoas");

            migrationBuilder.DropIndex(
                name: "IX_FornecedorCategorias_Nome",
                table: "FornecedorCategorias");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Produtos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Pessoas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "FornecedorCategorias",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                DO $$
                DECLARE default_company_id integer;
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM ""Empresas"") THEN
                        INSERT INTO ""Empresas"" (""Nome"", ""Documento"", ""EmailContato"", ""TelefoneContato"", ""Ativo"", ""CreatedAt"")
                        VALUES ('Empresa padrão', '00000000000000', 'contato@empresa.com', '', true, NOW())
                        RETURNING ""Id"" INTO default_company_id;
                    ELSE
                        SELECT ""Id"" INTO default_company_id FROM ""Empresas"" ORDER BY ""Id"" LIMIT 1;
                    END IF;

                    UPDATE ""Produtos"" SET ""EmpresaId"" = default_company_id WHERE ""EmpresaId"" IS NULL;
                    UPDATE ""Pessoas"" SET ""EmpresaId"" = default_company_id WHERE ""EmpresaId"" IS NULL;
                    UPDATE ""FornecedorCategorias"" SET ""EmpresaId"" = default_company_id WHERE ""EmpresaId"" IS NULL;
                END $$;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Produtos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Pessoas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "FornecedorCategorias",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_EmpresaId",
                table: "Produtos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_EmpresaId",
                table: "Pessoas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_EmpresaId_Documento",
                table: "Pessoas",
                columns: new[] { "EmpresaId", "Documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_EmpresaId_Email",
                table: "Pessoas",
                columns: new[] { "EmpresaId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FornecedorCategorias_EmpresaId",
                table: "FornecedorCategorias",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_FornecedorCategorias_EmpresaId_Nome",
                table: "FornecedorCategorias",
                columns: new[] { "EmpresaId", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FornecedorCategorias_Empresas_EmpresaId",
                table: "FornecedorCategorias",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoas_Empresas_EmpresaId",
                table: "Pessoas",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Empresas_EmpresaId",
                table: "Produtos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FornecedorCategorias_Empresas_EmpresaId",
                table: "FornecedorCategorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Pessoas_Empresas_EmpresaId",
                table: "Pessoas");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Empresas_EmpresaId",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "IX_Produtos_EmpresaId",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "IX_Pessoas_EmpresaId",
                table: "Pessoas");

            migrationBuilder.DropIndex(
                name: "IX_Pessoas_EmpresaId_Documento",
                table: "Pessoas");

            migrationBuilder.DropIndex(
                name: "IX_Pessoas_EmpresaId_Email",
                table: "Pessoas");

            migrationBuilder.DropIndex(
                name: "IX_FornecedorCategorias_EmpresaId",
                table: "FornecedorCategorias");

            migrationBuilder.DropIndex(
                name: "IX_FornecedorCategorias_EmpresaId_Nome",
                table: "FornecedorCategorias");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Pessoas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "FornecedorCategorias");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_Documento",
                table: "Pessoas",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_Email",
                table: "Pessoas",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FornecedorCategorias_Nome",
                table: "FornecedorCategorias",
                column: "Nome",
                unique: true);
        }
    }
}
