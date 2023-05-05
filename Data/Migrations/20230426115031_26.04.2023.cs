using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class _26042023 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Timer",
                table: "UserDate",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Philosophies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: false),
                    TheSaying = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Philosophies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Philosophies");

            migrationBuilder.DropColumn(
                name: "Timer",
                table: "UserDate");
        }
    }
}
