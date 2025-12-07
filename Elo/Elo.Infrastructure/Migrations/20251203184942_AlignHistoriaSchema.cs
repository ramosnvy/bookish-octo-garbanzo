using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AlignHistoriaSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Historias_Users_UsuarioResponsavelId",
                table: "Historias");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Tickets",
                newName: "TicketTipoId");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Historias",
                newName: "HistoriaTipoId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Historias",
                newName: "HistoriaStatusId");

            migrationBuilder.RenameColumn(
                name: "StatusNovo",
                table: "HistoriaMovimentacoes",
                newName: "StatusNovoId");

            migrationBuilder.RenameColumn(
                name: "StatusAnterior",
                table: "HistoriaMovimentacoes",
                newName: "StatusAnteriorId");

            migrationBuilder.AddColumn<int>(
                name: "FornecedorId",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioResponsavelId",
                table: "Historias",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "PrevisaoDias",
                table: "Historias",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistoriaStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: true),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Cor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FechaHistoria = table.Column<bool>(type: "boolean", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriaStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoriaStatuses_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoriaTipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: true),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriaTipos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoriaTipos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
INSERT INTO "HistoriaStatuses" ("Id", "Nome", "Descricao", "Cor", "FechaHistoria", "Ordem", "Ativo", "CreatedAt", "UpdatedAt")
VALUES
    (1, 'Pendente', NULL, NULL, FALSE, 1, TRUE, NOW(), NULL),
    (2, 'Em andamento', NULL, NULL, FALSE, 2, TRUE, NOW(), NULL),
    (3, 'Concluída', NULL, NULL, TRUE, 3, TRUE, NOW(), NULL),
    (4, 'Cancelada', NULL, NULL, TRUE, 4, TRUE, NOW(), NULL),
    (5, 'Pausada', NULL, NULL, FALSE, 5, TRUE, NOW(), NULL)
ON CONFLICT ("Id") DO NOTHING;
""");

            migrationBuilder.Sql("""
INSERT INTO "HistoriaTipos" ("Id", "Nome", "Descricao", "Ordem", "Ativo", "CreatedAt", "UpdatedAt")
VALUES
    (1, 'Projeto', NULL, 1, TRUE, NOW(), NULL),
    (2, 'Entrega', NULL, 2, TRUE, NOW(), NULL),
    (3, 'Operação', NULL, 3, TRUE, NOW(), NULL),
    (4, 'Implementação', NULL, 4, TRUE, NOW(), NULL),
    (5, 'Ordem de Serviço', NULL, 5, TRUE, NOW(), NULL)
ON CONFLICT ("Id") DO NOTHING;
""");

            migrationBuilder.CreateTable(
                name: "TicketAnexos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tamanho = table.Column<long>(type: "bigint", nullable: false),
                    Conteudo = table.Column<byte[]>(type: "bytea", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAnexos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAnexos_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketAnexos_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "TicketTipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: true),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTipos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTipos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FornecedorId",
                table: "Tickets",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProdutoId",
                table: "Tickets",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketTipoId",
                table: "Tickets",
                column: "TicketTipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Historias_HistoriaStatusId",
                table: "Historias",
                column: "HistoriaStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Historias_HistoriaTipoId",
                table: "Historias",
                column: "HistoriaTipoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriaMovimentacoes_StatusAnteriorId",
                table: "HistoriaMovimentacoes",
                column: "StatusAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriaMovimentacoes_StatusNovoId",
                table: "HistoriaMovimentacoes",
                column: "StatusNovoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriaStatuses_EmpresaId_Nome",
                table: "HistoriaStatuses",
                columns: new[] { "EmpresaId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoriaTipos_EmpresaId_Nome",
                table: "HistoriaTipos",
                columns: new[] { "EmpresaId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketAnexos_TicketId",
                table: "TicketAnexos",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAnexos_UsuarioId",
                table: "TicketAnexos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTipos_EmpresaId_Nome",
                table: "TicketTipos",
                columns: new[] { "EmpresaId", "Nome" },
                unique: true);

            migrationBuilder.Sql("""
INSERT INTO "TicketTipos" ("Id", "Nome", "Descricao", "Ordem", "Ativo", "CreatedAt", "UpdatedAt")
VALUES
    (1, 'Suporte', NULL, 1, TRUE, NOW(), NULL),
    (2, 'Bug', NULL, 2, TRUE, NOW(), NULL),
    (3, 'Melhoria', NULL, 3, TRUE, NOW(), NULL),
    (4, 'Dúvida', NULL, 4, TRUE, NOW(), NULL),
    (5, 'Incidente', NULL, 5, TRUE, NOW(), NULL)
ON CONFLICT ("Id") DO NOTHING;
""");

            migrationBuilder.Sql("""
SELECT setval('"HistoriaStatuses_Id_seq"', COALESCE((SELECT MAX("Id") FROM "HistoriaStatuses"), 0));
""");

            migrationBuilder.Sql("""
SELECT setval('"HistoriaTipos_Id_seq"', COALESCE((SELECT MAX("Id") FROM "HistoriaTipos"), 0));
""");

            migrationBuilder.Sql("""
SELECT setval('"TicketTipos_Id_seq"', COALESCE((SELECT MAX("Id") FROM "TicketTipos"), 0));
""");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoriaMovimentacoes_HistoriaStatuses_StatusAnteriorId",
                table: "HistoriaMovimentacoes",
                column: "StatusAnteriorId",
                principalTable: "HistoriaStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoriaMovimentacoes_HistoriaStatuses_StatusNovoId",
                table: "HistoriaMovimentacoes",
                column: "StatusNovoId",
                principalTable: "HistoriaStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Historias_HistoriaStatuses_HistoriaStatusId",
                table: "Historias",
                column: "HistoriaStatusId",
                principalTable: "HistoriaStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Historias_HistoriaTipos_HistoriaTipoId",
                table: "Historias",
                column: "HistoriaTipoId",
                principalTable: "HistoriaTipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Historias_Users_UsuarioResponsavelId",
                table: "Historias",
                column: "UsuarioResponsavelId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Pessoas_FornecedorId",
                table: "Tickets",
                column: "FornecedorId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Produtos_ProdutoId",
                table: "Tickets",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TicketTipos_TicketTipoId",
                table: "Tickets",
                column: "TicketTipoId",
                principalTable: "TicketTipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoriaMovimentacoes_HistoriaStatuses_StatusAnteriorId",
                table: "HistoriaMovimentacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoriaMovimentacoes_HistoriaStatuses_StatusNovoId",
                table: "HistoriaMovimentacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Historias_HistoriaStatuses_HistoriaStatusId",
                table: "Historias");

            migrationBuilder.DropForeignKey(
                name: "FK_Historias_HistoriaTipos_HistoriaTipoId",
                table: "Historias");

            migrationBuilder.DropForeignKey(
                name: "FK_Historias_Users_UsuarioResponsavelId",
                table: "Historias");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Pessoas_FornecedorId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Produtos_ProdutoId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TicketTipos_TicketTipoId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "HistoriaStatuses");

            migrationBuilder.DropTable(
                name: "HistoriaTipos");

            migrationBuilder.DropTable(
                name: "TicketAnexos");

            migrationBuilder.DropTable(
                name: "TicketTipos");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_FornecedorId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ProdutoId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TicketTipoId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Historias_HistoriaStatusId",
                table: "Historias");

            migrationBuilder.DropIndex(
                name: "IX_Historias_HistoriaTipoId",
                table: "Historias");

            migrationBuilder.DropIndex(
                name: "IX_HistoriaMovimentacoes_StatusAnteriorId",
                table: "HistoriaMovimentacoes");

            migrationBuilder.DropIndex(
                name: "IX_HistoriaMovimentacoes_StatusNovoId",
                table: "HistoriaMovimentacoes");

            migrationBuilder.DropColumn(
                name: "FornecedorId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PrevisaoDias",
                table: "Historias");

            migrationBuilder.RenameColumn(
                name: "TicketTipoId",
                table: "Tickets",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "HistoriaTipoId",
                table: "Historias",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "HistoriaStatusId",
                table: "Historias",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "StatusNovoId",
                table: "HistoriaMovimentacoes",
                newName: "StatusNovo");

            migrationBuilder.RenameColumn(
                name: "StatusAnteriorId",
                table: "HistoriaMovimentacoes",
                newName: "StatusAnterior");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioResponsavelId",
                table: "Historias",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Historias_Users_UsuarioResponsavelId",
                table: "Historias",
                column: "UsuarioResponsavelId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
