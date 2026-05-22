using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDescricaoImagemCorte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Corte",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Corte",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "Corte",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Corte");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Corte");

            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Corte");
        }
    }
}
