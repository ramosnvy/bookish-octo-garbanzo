using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloArrayToFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<int>>(
                name: "ProdutoModuloIds",
                table: "ContaReceberItens",
                type: "integer[]",
                nullable: false,
                defaultValueSql: "'{}'::integer[]");

            migrationBuilder.AddColumn<List<int>>(
                name: "ProdutoModuloIds",
                table: "ContaPagarItens",
                type: "integer[]",
                nullable: false,
                defaultValueSql: "'{}'::integer[]");

            migrationBuilder.Sql(
                """
                UPDATE "ContaReceberItens"
                SET "ProdutoModuloIds" = ARRAY["ProdutoModuloId"]
                WHERE "ProdutoModuloId" IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "ContaPagarItens"
                SET "ProdutoModuloIds" = ARRAY["ProdutoModuloId"]
                WHERE "ProdutoModuloId" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "ProdutoModuloId",
                table: "ContaReceberItens");

            migrationBuilder.DropColumn(
                name: "ProdutoModuloId",
                table: "ContaPagarItens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProdutoModuloId",
                table: "ContaReceberItens",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProdutoModuloId",
                table: "ContaPagarItens",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "ContaReceberItens"
                SET "ProdutoModuloId" = "ProdutoModuloIds"[1]
                WHERE "ProdutoModuloIds" IS NOT NULL AND cardinality("ProdutoModuloIds") > 0;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "ContaPagarItens"
                SET "ProdutoModuloId" = "ProdutoModuloIds"[1]
                WHERE "ProdutoModuloIds" IS NOT NULL AND cardinality("ProdutoModuloIds") > 0;
                """);

            migrationBuilder.DropColumn(
                name: "ProdutoModuloIds",
                table: "ContaReceberItens");

            migrationBuilder.DropColumn(
                name: "ProdutoModuloIds",
                table: "ContaPagarItens");
        }
    }
}
