using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HellavorX.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Posts_PostId",
                table: "MediaFiles");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "MediaFiles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CommentId",
                table: "MediaFiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_CommentId",
                table: "MediaFiles",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Comments_CommentId",
                table: "MediaFiles",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Posts_PostId",
                table: "MediaFiles",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Comments_CommentId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Posts_PostId",
                table: "MediaFiles");

            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_CommentId",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "MediaFiles");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "MediaFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Posts_PostId",
                table: "MediaFiles",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
