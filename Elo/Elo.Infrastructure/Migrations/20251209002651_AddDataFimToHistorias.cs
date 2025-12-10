using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddDataFimToHistorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataFim",
                table: "Historias",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Assinaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsRecorrente = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IntervaloDias = table.Column<int>(type: "integer", nullable: true),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GerarFinanceiro = table.Column<bool>(type: "boolean", nullable: false),
                    GerarImplantacao = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Pessoas_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssinaturaItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssinaturaId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoModuloId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssinaturaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssinaturaItens_Assinaturas_AssinaturaId",
                        column: x => x.AssinaturaId,
                        principalTable: "Assinaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssinaturaItens_ProdutoModulos_ProdutoModuloId",
                        column: x => x.ProdutoModuloId,
                        principalTable: "ProdutoModulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssinaturaItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaItens_AssinaturaId",
                table: "AssinaturaItens",
                column: "AssinaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaItens_ProdutoId",
                table: "AssinaturaItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaItens_ProdutoModuloId",
                table: "AssinaturaItens",
                column: "ProdutoModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_ClienteId",
                table: "Assinaturas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_EmpresaId",
                table: "Assinaturas",
                column: "EmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssinaturaItens");

            migrationBuilder.DropTable(
                name: "Assinaturas");

            migrationBuilder.DropColumn(
                name: "DataFim",
                table: "Historias");
        }
    }
}
