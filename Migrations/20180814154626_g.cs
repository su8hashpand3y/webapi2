using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebApi1.Migrations
{
    public partial class g : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ans3",
                table: "User",
                newName: "FavNumber");

            migrationBuilder.RenameColumn(
                name: "Ans2",
                table: "User",
                newName: "FavMonth");

            migrationBuilder.RenameColumn(
                name: "Ans1",
                table: "User",
                newName: "FavColor");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FavNumber",
                table: "User",
                newName: "Ans3");

            migrationBuilder.RenameColumn(
                name: "FavMonth",
                table: "User",
                newName: "Ans2");

            migrationBuilder.RenameColumn(
                name: "FavColor",
                table: "User",
                newName: "Ans1");
        }
    }
}
