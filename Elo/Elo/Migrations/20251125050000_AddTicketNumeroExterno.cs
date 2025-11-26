using Elo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elo.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20251125050000_AddTicketNumeroExterno")]
    public partial class AddTicketNumeroExterno : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroExterno",
                table: "Tickets",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: string.Empty);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroExterno",
                table: "Tickets");
        }
    }
}
