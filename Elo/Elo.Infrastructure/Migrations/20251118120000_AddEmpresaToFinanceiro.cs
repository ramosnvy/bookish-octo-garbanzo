using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaToFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pessoas_EmpresaId",
                table: "Pessoas");

            migrationBuilder.DropIndex(
                name: "IX_FornecedorCategorias_EmpresaId",
                table: "FornecedorCategorias");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "ContasReceber",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervaloDias",
                table: "ContasReceber",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecorrente",
                table: "ContasReceber",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalParcelas",
                table: "ContasReceber",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "ContasPagar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervaloDias",
                table: "ContasPagar",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecorrente",
                table: "ContasPagar",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalParcelas",
                table: "ContasPagar",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE ""ContasReceber"" cr
                SET ""EmpresaId"" = p.""EmpresaId""
                FROM ""Pessoas"" p
                WHERE cr.""ClienteId"" = p.""Id"";
            ");

            migrationBuilder.Sql(@"
                DO $$
                DECLARE default_company_id integer;
                BEGIN
                    SELECT ""Id"" INTO default_company_id FROM ""Empresas"" ORDER BY ""Id"" LIMIT 1;
                    UPDATE ""ContasReceber"" SET ""EmpresaId"" = default_company_id WHERE ""EmpresaId"" IS NULL;
                END $$;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""ContasPagar"" cp
                SET ""EmpresaId"" = p.""EmpresaId""
                FROM ""Pessoas"" p
                WHERE cp.""FornecedorId"" = p.""Id"";
            ");

            migrationBuilder.Sql(@"
                DO $$
                DECLARE default_company_id integer;
                BEGIN
                    SELECT ""Id"" INTO default_company_id FROM ""Empresas"" ORDER BY ""Id"" LIMIT 1;
                    UPDATE ""ContasPagar"" SET ""EmpresaId"" = default_company_id WHERE ""EmpresaId"" IS NULL;
                END $$;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "ContasReceber",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "ContasPagar",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ContaPagarItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ContaPagarId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: true),
                    ProdutoModuloId = table.Column<int>(type: "integer", nullable: true),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContaPagarItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContaPagarItens_ContasPagar_ContaPagarId",
                        column: x => x.ContaPagarId,
                        principalTable: "ContasPagar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContaPagarParcelas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ContaPagarId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContaPagarParcelas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContaPagarParcelas_ContasPagar_ContaPagarId",
                        column: x => x.ContaPagarId,
                        principalTable: "ContasPagar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContaReceberItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ContaReceberId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: true),
                    ProdutoModuloId = table.Column<int>(type: "integer", nullable: true),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContaReceberItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContaReceberItens_ContasReceber_ContaReceberId",
                        column: x => x.ContaReceberId,
                        principalTable: "ContasReceber",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContaReceberParcelas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ContaReceberId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContaReceberParcelas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContaReceberParcelas_ContasReceber_ContaReceberId",
                        column: x => x.ContaReceberId,
                        principalTable: "ContasReceber",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceber_EmpresaId",
                table: "ContasReceber",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_EmpresaId",
                table: "ContasPagar",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContaPagarItens_ContaPagarId",
                table: "ContaPagarItens",
                column: "ContaPagarId");

            migrationBuilder.CreateIndex(
                name: "IX_ContaPagarParcelas_ContaPagarId",
                table: "ContaPagarParcelas",
                column: "ContaPagarId");

            migrationBuilder.CreateIndex(
                name: "IX_ContaReceberItens_ContaReceberId",
                table: "ContaReceberItens",
                column: "ContaReceberId");

            migrationBuilder.CreateIndex(
                name: "IX_ContaReceberParcelas_ContaReceberId",
                table: "ContaReceberParcelas",
                column: "ContaReceberId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContasPagar_Empresas_EmpresaId",
                table: "ContasPagar",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContasReceber_Empresas_EmpresaId",
                table: "ContasReceber",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContasPagar_Empresas_EmpresaId",
                table: "ContasPagar");

            migrationBuilder.DropForeignKey(
                name: "FK_ContasReceber_Empresas_EmpresaId",
                table: "ContasReceber");

            migrationBuilder.DropTable(
                name: "ContaPagarItens");

            migrationBuilder.DropTable(
                name: "ContaPagarParcelas");

            migrationBuilder.DropTable(
                name: "ContaReceberItens");

            migrationBuilder.DropTable(
                name: "ContaReceberParcelas");

            migrationBuilder.DropIndex(
                name: "IX_ContasReceber_EmpresaId",
                table: "ContasReceber");

            migrationBuilder.DropIndex(
                name: "IX_ContasPagar_EmpresaId",
                table: "ContasPagar");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "ContasReceber");

            migrationBuilder.DropColumn(
                name: "IntervaloDias",
                table: "ContasReceber");

            migrationBuilder.DropColumn(
                name: "IsRecorrente",
                table: "ContasReceber");

            migrationBuilder.DropColumn(
                name: "TotalParcelas",
                table: "ContasReceber");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "ContasPagar");

            migrationBuilder.DropColumn(
                name: "IntervaloDias",
                table: "ContasPagar");

            migrationBuilder.DropColumn(
                name: "IsRecorrente",
                table: "ContasPagar");

            migrationBuilder.DropColumn(
                name: "TotalParcelas",
                table: "ContasPagar");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_EmpresaId",
                table: "Pessoas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_FornecedorCategorias_EmpresaId",
                table: "FornecedorCategorias",
                column: "EmpresaId");
        }
    }
}
