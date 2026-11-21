using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAboutToSystemSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AboutDescription",
                table: "SystemSettings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AboutTitle",
                table: "SystemSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AboutDescription",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "AboutTitle",
                table: "SystemSettings");
        }
    }
}
