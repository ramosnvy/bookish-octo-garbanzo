using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddFormaPagamentoToAssinatura2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormaPagamento",
                table: "Assinaturas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmpresaFormasPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    FormaPagamento = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaFormasPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaFormasPagamento_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaFormasPagamento_EmpresaId_FormaPagamento",
                table: "EmpresaFormasPagamento",
                columns: new[] { "EmpresaId", "FormaPagamento" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresaFormasPagamento");

            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "Assinaturas");
        }
    }
}
