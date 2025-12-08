using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class EmpresaExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TelefoneContato",
                table: "Empresas",
                newName: "Telefone");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Empresas",
                newName: "RazaoSocial");

            migrationBuilder.RenameColumn(
                name: "EmailContato",
                table: "Empresas",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "Documento",
                table: "Empresas",
                newName: "Cnpj");

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Empresas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomeFantasia",
                table: "Empresas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InscricaoEstadual",
                table: "Empresas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE ""Empresas"" SET ""NomeFantasia"" = ""RazaoSocial"" WHERE ""NomeFantasia"" = '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "NomeFantasia",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "InscricaoEstadual",
                table: "Empresas");

            migrationBuilder.RenameColumn(
                name: "Telefone",
                table: "Empresas",
                newName: "TelefoneContato");

            migrationBuilder.RenameColumn(
                name: "RazaoSocial",
                table: "Empresas",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Empresas",
                newName: "EmailContato");

            migrationBuilder.RenameColumn(
                name: "Cnpj",
                table: "Empresas",
                newName: "Documento");
        }
    }
}
