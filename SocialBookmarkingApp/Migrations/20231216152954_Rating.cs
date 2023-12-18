using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialBookmarkingApp.Data.Migrations
{
    public partial class Rating : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_ApplicationUserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Bookmarks_BookmarkId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Reviews",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ApplicationUserId",
                table: "Reviews",
                newName: "IX_Reviews_UserId");

            migrationBuilder.AlterColumn<int>(
                name: "BookmarkId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "Content",
                keyValue: null,
                column: "Content",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Bookmarks_BookmarkId",
                table: "Reviews",
                column: "BookmarkId",
                principalTable: "Bookmarks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_UserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Bookmarks_BookmarkId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reviews",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                newName: "IX_Reviews_ApplicationUserId");

            migrationBuilder.AlterColumn<int>(
                name: "BookmarkId",
                table: "Reviews",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Reviews",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Reviews",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_ApplicationUserId",
                table: "Reviews",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Bookmarks_BookmarkId",
                table: "Reviews",
                column: "BookmarkId",
                principalTable: "Bookmarks",
                principalColumn: "Id");
        }
    }
}
