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
                name: "ReportEntity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    EntityId = table.Column<int>(nullable: false),
                    ReportedEntityRefference = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    CreatedByUserId = table.Column<string>(nullable: true),
                    AddresedMessage = table.Column<string>(nullable: true),
                    IsAddressed = table.Column<short>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    TicketIssuerUserId = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    IsAddressed = table.Column<short>(nullable: false),
                    AddresedMessage = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
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
                        principalColumn: "UpdatedPostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketFile",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    FilePath = table.Column<string>(nullable: true),
                    ReportedEntityTypeId = table.Column<int>(nullable: false),
                    ReportEntityId = table.Column<int>(nullable: true),
                    TicketsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketFiles_ReportEntity_ReportEntityId",
                        column: x => x.ReportEntityId,
                        principalTable: "ReportEntity",
                        principalColumn: "UpdatedPostId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketFiles_Tickets_TicketsId",
                        column: x => x.TicketsId,
                        principalTable: "Ticket",
                        principalColumn: "UpdatedPostId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_Files_CommentId",
                table: "Comment_Files",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostFiles_PostId",
                table: "PostFiles",
                column: "UpdatedPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketFiles_ReportEntityId",
                table: "TicketFile",
                column: "ReportEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketFiles_TicketsId",
                table: "TicketFile",
                column: "TicketsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment_Files");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "PostFiles");

            migrationBuilder.DropTable(
                name: "PostLikes");

            migrationBuilder.DropTable(
                name: "TicketFile");

            migrationBuilder.DropTable(
                name: "UserCommonData");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "ReportEntity");

            migrationBuilder.DropTable(
                name: "Ticket");
        }
    }
}
