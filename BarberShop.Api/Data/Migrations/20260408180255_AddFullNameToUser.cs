using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_IdentityUser_UserId",
                table: "IdentityRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRole_UserId",
                table: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "NomeCompleto",
                table: "IdentityUser");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "IdentityRole");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityUser_NormalizedUserName",
                table: "IdentityUser",
                newName: "UserNameIndex");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityUser_NormalizedEmail",
                table: "IdentityUser",
                newName: "EmailIndex");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityRole_NormalizedName",
                table: "IdentityRole",
                newName: "RoleNameIndex");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "IdentityUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityUserRole_RoleId",
                table: "IdentityUserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRoleClaim_RoleId",
                table: "IdentityRoleClaim",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRoleClaim_IdentityRole_RoleId",
                table: "IdentityRoleClaim",
                column: "RoleId",
                principalTable: "IdentityRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserRole_IdentityRole_RoleId",
                table: "IdentityUserRole",
                column: "RoleId",
                principalTable: "IdentityRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRoleClaim_IdentityRole_RoleId",
                table: "IdentityRoleClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUserRole_IdentityRole_RoleId",
                table: "IdentityUserRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityUserRole_RoleId",
                table: "IdentityUserRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRoleClaim_RoleId",
                table: "IdentityRoleClaim");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "IdentityUser");

            migrationBuilder.RenameIndex(
                name: "UserNameIndex",
                table: "IdentityUser",
                newName: "IX_IdentityUser_NormalizedUserName");

            migrationBuilder.RenameIndex(
                name: "EmailIndex",
                table: "IdentityUser",
                newName: "IX_IdentityUser_NormalizedEmail");

            migrationBuilder.RenameIndex(
                name: "RoleNameIndex",
                table: "IdentityRole",
                newName: "IX_IdentityRole_NormalizedName");

            migrationBuilder.AddColumn<string>(
                name: "NomeCompleto",
                table: "IdentityUser",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "IdentityRole",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_UserId",
                table: "IdentityRole",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRole_IdentityUser_UserId",
                table: "IdentityRole",
                column: "UserId",
                principalTable: "IdentityUser",
                principalColumn: "Id");
        }
    }
}
