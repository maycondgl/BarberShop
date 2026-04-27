using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendamentoIdToAvaliacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "IdentityUser");

            migrationBuilder.AddColumn<long>(
                name: "AgendamentoId",
                table: "Avaliacao",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgendamentoId",
                table: "Avaliacao");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "IdentityUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
