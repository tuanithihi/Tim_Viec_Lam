using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimViecLam.Migrations
{
    /// <inheritdoc />
    public partial class AddUserResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiresAt",
                table: "tblUser",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetTokenHash",
                table: "tblUser",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetTokenExpiresAt",
                table: "tblUser");

            migrationBuilder.DropColumn(
                name: "ResetTokenHash",
                table: "tblUser");
        }
    }
}
