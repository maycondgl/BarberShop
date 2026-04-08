using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNumeroTelefone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroTelefone",
                table: "IdentityUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroTelefone",
                table: "IdentityUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
