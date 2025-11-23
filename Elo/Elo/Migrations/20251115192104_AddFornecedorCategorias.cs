using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddFornecedorCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FornecedorCategoriaId",
                table: "Pessoas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FornecedorCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FornecedorCategorias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_FornecedorCategoriaId",
                table: "Pessoas",
                column: "FornecedorCategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_FornecedorCategorias_Nome",
                table: "FornecedorCategorias",
                column: "Nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoas_FornecedorCategorias_FornecedorCategoriaId",
                table: "Pessoas",
                column: "FornecedorCategoriaId",
                principalTable: "FornecedorCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pessoas_FornecedorCategorias_FornecedorCategoriaId",
                table: "Pessoas");

            migrationBuilder.DropTable(
                name: "FornecedorCategorias");

            migrationBuilder.DropIndex(
                name: "IX_Pessoas_FornecedorCategoriaId",
                table: "Pessoas");

            migrationBuilder.DropColumn(
                name: "FornecedorCategoriaId",
                table: "Pessoas");
        }
    }
}
