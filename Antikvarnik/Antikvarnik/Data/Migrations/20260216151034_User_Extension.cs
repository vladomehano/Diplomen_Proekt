using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Antikvarnik.Data.Migrations
{
    /// <inheritdoc />
    public partial class User_Extension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavoriteNumber",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FavoriteNumber",
                table: "AspNetUsers");
        }
    }
}
