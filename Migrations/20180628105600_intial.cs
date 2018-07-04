using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebApi1.Migrations
{
    public partial class intial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inbox",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsMyMessage = table.Column<bool>(nullable: false),
                    IsRead = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    MessageGroupUniqueGuid = table.Column<Guid>(nullable: false),
                    UserUniqueId = table.Column<string>(nullable: true)
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
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsMyMessage = table.Column<bool>(nullable: false),
                    IsRead = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    MessageGroupUniqueGuid = table.Column<Guid>(nullable: false)
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
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsAnonymousAllowed = table.Column<bool>(nullable: false),
                    IsPublicSearchAvailable = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    PhotoUrl = table.Column<string>(nullable: true),
                    Salt = table.Column<string>(nullable: true),
                    SecurityAns1 = table.Column<string>(nullable: true),
                    SecurityAns2 = table.Column<string>(nullable: true),
                    SecurityAns3 = table.Column<string>(nullable: true),
                    SecurityQue1 = table.Column<string>(nullable: true),
                    SecurityQue2 = table.Column<string>(nullable: true),
                    SecurityQue3 = table.Column<string>(nullable: true),
                    UserUniqueId = table.Column<string>(nullable: true)
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
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(nullable: false),
                    MessageGroupUniqueGuid = table.Column<Guid>(nullable: false),
                    UserUniqueId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessage", x => x.Id);
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
        }
    }
}
