using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamento_Cliente_ClienteId",
                table: "Agendamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacao_Cliente_ClienteId",
                table: "Avaliacao");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacao_ClienteId",
                table: "Avaliacao");

            migrationBuilder.DropIndex(
                name: "IX_Agendamento_ClienteId",
                table: "Agendamento");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Avaliacao");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Agendamento");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Avaliacao",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Agendamento",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_UserId",
                table: "Avaliacao",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamento_UserId",
                table: "Agendamento",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamento_IdentityUser_UserId",
                table: "Agendamento",
                column: "UserId",
                principalTable: "IdentityUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacao_IdentityUser_UserId",
                table: "Avaliacao",
                column: "UserId",
                principalTable: "IdentityUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamento_IdentityUser_UserId",
                table: "Agendamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacao_IdentityUser_UserId",
                table: "Avaliacao");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacao_UserId",
                table: "Avaliacao");

            migrationBuilder.DropIndex(
                name: "IX_Agendamento_UserId",
                table: "Agendamento");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Avaliacao",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ClienteId",
                table: "Avaliacao",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Agendamento",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ClienteId",
                table: "Agendamento",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_ClienteId",
                table: "Avaliacao",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamento_ClienteId",
                table: "Agendamento",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamento_Cliente_ClienteId",
                table: "Agendamento",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacao_Cliente_ClienteId",
                table: "Avaliacao",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
