using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialBookmarkingApp.Migrations
{
    public partial class BookmarkCategory2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserBookmark");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUserBookmark",
                columns: table => new
                {
                    SavedBookmarksId = table.Column<int>(type: "int", nullable: false),
                    SavedById = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserBookmark", x => new { x.SavedBookmarksId, x.SavedById });
                    table.ForeignKey(
                        name: "FK_ApplicationUserBookmark_AspNetUsers_SavedById",
                        column: x => x.SavedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserBookmark_Bookmarks_SavedBookmarksId",
                        column: x => x.SavedBookmarksId,
                        principalTable: "Bookmarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserBookmark_SavedById",
                table: "ApplicationUserBookmark",
                column: "SavedById");
        }
    }
}
