using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuickMenuToSystemSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuickMenuBackgroundFileId",
                table: "SystemSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "QuickMenuEnabled",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "QuickMenuItems",
                table: "SystemSettings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_QuickMenuBackgroundFileId",
                table: "SystemSettings",
                column: "QuickMenuBackgroundFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemSettings_Files_QuickMenuBackgroundFileId",
                table: "SystemSettings",
                column: "QuickMenuBackgroundFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SystemSettings_Files_QuickMenuBackgroundFileId",
                table: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_QuickMenuBackgroundFileId",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "QuickMenuBackgroundFileId",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "QuickMenuEnabled",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "QuickMenuItems",
                table: "SystemSettings");
        }
    }
}
