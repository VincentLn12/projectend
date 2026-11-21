using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRegistrationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShortCourseRegistrations_Code",
                table: "ShortCourseRegistrations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ShortCourseRegistrations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ShortCourseRegistrations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ShortCourseRegistrations_Code",
                table: "ShortCourseRegistrations",
                column: "Code",
                unique: true);
        }
    }
}
