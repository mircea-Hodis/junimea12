using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthWebApi.Migrations.MysqlDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comment_Files",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    CommentId = table.Column<long>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommentLikes",
                columns: table => new
                {
                    LikeId = table.Column<long>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    UserId = table.Column<string>(nullable: true),
                    CommentId = table.Column<long>(nullable: false),
                    LikeTime = table.Column<DateTime>(nullable: false),
                    LikeCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentLikes", x => x.LikeId);
                });

            migrationBuilder.CreateTable(
                name: "CommentReports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    PostId = table.Column<int>(nullable: false),
                    EntityId = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    ReportedByUserId = table.Column<string>(nullable: true),
                    AddressedMessage = table.Column<string>(nullable: true),
                    AddressedById = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    AddresDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    PostId = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Likes = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(maxLength: 50, nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostLikes",
                columns: table => new
                {
                    LikeId = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    UserId = table.Column<string>(nullable: true),
                    PostId = table.Column<int>(nullable: false),
                    LikeTime = table.Column<DateTime>(nullable: false),
                    LikeCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLikes", x => x.LikeId);
                });

            migrationBuilder.CreateTable(
                name: "PostReports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    EntityId = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    ReportedByUserId = table.Column<string>(nullable: true),
                    AddressedMessage = table.Column<string>(nullable: true),
                    AddressedById = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(2019, 3, 14, 20, 13, 27, 817, DateTimeKind.Local)),
                    AddresDateTime = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(2019, 3, 14, 20, 13, 27, 809, DateTimeKind.Local))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    PostTtile = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Likes = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    TicketIssuerUserId = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    IsPending = table.Column<short>(nullable: false),
                    IsAddressed = table.Column<short>(nullable: false),
                    AddressedMessage = table.Column<string>(nullable: true),
                    AddressedById = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCommonData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    UserId = table.Column<string>(nullable: true),
                    FacebookId = table.Column<long>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: true),
                    UserEmail = table.Column<string>(nullable: true),
                    UserLevel = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCommonData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    PostId = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostFiles_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_Files_CommentId",
                table: "Comment_Files",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_CommentId",
                table: "CommentLikes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostFiles_PostId",
                table: "PostFiles",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment_Files");

            migrationBuilder.DropTable(
                name: "CommentLikes");

            migrationBuilder.DropTable(
                name: "CommentReports");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "PostFiles");

            migrationBuilder.DropTable(
                name: "PostLikes");

            migrationBuilder.DropTable(
                name: "PostReports");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "UserCommonData");

            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}
