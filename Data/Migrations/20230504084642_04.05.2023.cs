using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class _04052023 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFixedDay",
                table: "UserDate",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFixedDay",
                table: "UserDate");
        }
    }
}
