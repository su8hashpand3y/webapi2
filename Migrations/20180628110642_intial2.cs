using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebApi1.Migrations
{
    public partial class intial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPublicSearchAvailable",
                table: "User",
                newName: "PublicSearchNotAvailable");

            migrationBuilder.RenameColumn(
                name: "IsAnonymousAllowed",
                table: "User",
                newName: "AnonymousNotAllowed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublicSearchNotAvailable",
                table: "User",
                newName: "IsPublicSearchAvailable");

            migrationBuilder.RenameColumn(
                name: "AnonymousNotAllowed",
                table: "User",
                newName: "IsAnonymousAllowed");
        }
    }
}
