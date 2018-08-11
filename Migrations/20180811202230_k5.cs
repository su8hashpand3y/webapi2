using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebApi1.Migrations
{
    public partial class k5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salt",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SecurityAns1",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SecurityAns2",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SecurityAns3",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "SecurityQue3",
                table: "User",
                newName: "Ans3");

            migrationBuilder.RenameColumn(
                name: "SecurityQue2",
                table: "User",
                newName: "Ans2");

            migrationBuilder.RenameColumn(
                name: "SecurityQue1",
                table: "User",
                newName: "Ans1");

            migrationBuilder.AddColumn<bool>(
                name: "IsFav",
                table: "UserMessage",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ReplyMessageInfo",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsFav = table.Column<bool>(nullable: false),
                    MessageGroupUniqueGuid = table.Column<Guid>(nullable: false),
                    UserUniqueId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplyMessageInfo", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReplyMessageInfo");

            migrationBuilder.DropColumn(
                name: "IsFav",
                table: "UserMessage");

            migrationBuilder.RenameColumn(
                name: "Ans3",
                table: "User",
                newName: "SecurityQue3");

            migrationBuilder.RenameColumn(
                name: "Ans2",
                table: "User",
                newName: "SecurityQue2");

            migrationBuilder.RenameColumn(
                name: "Ans1",
                table: "User",
                newName: "SecurityQue1");

            migrationBuilder.AddColumn<string>(
                name: "Salt",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityAns1",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityAns2",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityAns3",
                table: "User",
                nullable: true);
        }
    }
}
