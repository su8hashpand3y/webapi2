using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WebApi1.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inbox",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    MessageGroupUniqueGuid = table.Column<Guid>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    UserUniqueId = table.Column<string>(nullable: true),
                    UserIdentifier = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsRead = table.Column<bool>(nullable: false),
                    IsMyMessage = table.Column<bool>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reply",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    MessageGroupUniqueGuid = table.Column<Guid>(nullable: false),
                    UserUniqueId = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsRead = table.Column<bool>(nullable: false),
                    IsMyMessage = table.Column<bool>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reply", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    UserUniqueId = table.Column<string>(nullable: true),
                    PhotoUrl = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    AnonymousNotAllowed = table.Column<bool>(nullable: false),
                    PublicSearchNotAvailable = table.Column<bool>(nullable: false),
                    FavColor = table.Column<string>(nullable: true),
                    FavMonth = table.Column<string>(nullable: true),
                    FavNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMessage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    MessageGroupUniqueGuid = table.Column<Guid>(nullable: false),
                    UserUniqueId = table.Column<string>(nullable: true),
                    IsFav = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserReply",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    MessageGroupUniqueGuid = table.Column<Guid>(nullable: false),
                    UserUniqueId = table.Column<string>(nullable: true),
                    IsFav = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReply", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inbox");

            migrationBuilder.DropTable(
                name: "Reply");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "UserMessage");

            migrationBuilder.DropTable(
                name: "UserReply");
        }
    }
}
