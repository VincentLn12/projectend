using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // บัญชีที่สมัครไว้และยังรออนุมัติ ให้ได้บทบาท Student ก่อนลบคอลัมน์ ไม่งั้นจะใช้งานไม่ได้
            migrationBuilder.Sql(@"
                INSERT INTO UserRoles (UserId, RoleId)
                SELECT profile.UserId, role.Id
                FROM StudentProfiles AS profile
                CROSS JOIN Roles AS role
                WHERE profile.IsApproved = 0
                  AND role.NormalizedName = 'STUDENT'
                  AND NOT EXISTS (
                      SELECT 1 FROM UserRoles AS userRole
                      WHERE userRole.UserId = profile.UserId AND userRole.RoleId = role.Id);");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "StudentProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "StudentProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
