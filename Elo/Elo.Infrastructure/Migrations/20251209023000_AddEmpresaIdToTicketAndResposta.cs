using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaIdToTicketAndResposta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "RespostasTicket",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EmpresaId",
                table: "Tickets",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasTicket_EmpresaId",
                table: "RespostasTicket",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_RespostasTicket_Empresas_EmpresaId",
                table: "RespostasTicket",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Empresas_EmpresaId",
                table: "Tickets",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RespostasTicket_Empresas_EmpresaId",
                table: "RespostasTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Empresas_EmpresaId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_EmpresaId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_RespostasTicket_EmpresaId",
                table: "RespostasTicket");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "RespostasTicket");
        }
    }
}
