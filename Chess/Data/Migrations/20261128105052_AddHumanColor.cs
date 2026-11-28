using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chess.Migrations
{
    /// <inheritdoc />
    public partial class AddHumanColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HumanColor",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HumanColor",
                table: "Games");
        }
    }
}
