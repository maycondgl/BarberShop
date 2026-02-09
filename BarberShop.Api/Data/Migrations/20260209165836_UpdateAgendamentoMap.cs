using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAgendamentoMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorPago",
                table: "Agendamento");

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "Agendamento",
                type: "DECIMAL(18,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Valor",
                table: "Agendamento");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorPago",
                table: "Agendamento",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
