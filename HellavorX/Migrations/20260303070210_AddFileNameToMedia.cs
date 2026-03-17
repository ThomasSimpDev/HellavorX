using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HellavorX.Migrations
{
    /// <inheritdoc />
    public partial class AddFileNameToMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "MediaFiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "MediaFiles");
        }
    }
}
