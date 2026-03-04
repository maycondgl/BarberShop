using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToCorte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Corte",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Corte");
        }
    }
}
