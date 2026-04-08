using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNomeCompletoToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "IdentityUser",
                newName: "NumeroTelefone");

            migrationBuilder.AddColumn<string>(
                name: "NomeCompleto",
                table: "IdentityUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeCompleto",
                table: "IdentityUser");

            migrationBuilder.RenameColumn(
                name: "NumeroTelefone",
                table: "IdentityUser",
                newName: "FullName");
        }
    }
}
