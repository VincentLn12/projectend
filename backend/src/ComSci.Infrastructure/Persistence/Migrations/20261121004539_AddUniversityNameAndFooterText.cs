using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniversityNameAndFooterText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FooterText",
                table: "SystemSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniversityName",
                table: "SystemSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            // เว็บของ ม.ราชภัฏกาญจนบุรีที่ใช้งานอยู่แล้ว ให้ข้อความท้ายเว็บเหมือนเดิม (สาขาอื่นที่ติดตั้งใหม่ตารางยังว่าง ไม่กระทบ)
            migrationBuilder.Sql("UPDATE SystemSettings SET UniversityName = N'มหาวิทยาลัยราชภัฏกาญจนบุรี' WHERE UniversityName IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FooterText",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "UniversityName",
                table: "SystemSettings");
        }
    }
}
