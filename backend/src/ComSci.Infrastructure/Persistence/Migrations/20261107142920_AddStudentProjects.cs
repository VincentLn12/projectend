using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcademicYear = table.Column<short>(type: "smallint", nullable: false),
                    Semester = table.Column<byte>(type: "tinyint", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Technologies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TeamMembers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LecturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImageFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PdfFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GithubUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProjects_Files_ImageFileId",
                        column: x => x.ImageFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProjects_Files_PdfFileId",
                        column: x => x.PdfFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProjects_Lecturers_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Lecturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProjects_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProjects_AcademicYear_Semester_IsPublished_IsDeleted",
                table: "StudentProjects",
                columns: new[] { "AcademicYear", "Semester", "IsPublished", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProjects_ImageFileId",
                table: "StudentProjects",
                column: "ImageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProjects_LecturerId",
                table: "StudentProjects",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProjects_PdfFileId",
                table: "StudentProjects",
                column: "PdfFileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProjects_StudentId_AcademicYear_Semester",
                table: "StudentProjects",
                columns: new[] { "StudentId", "AcademicYear", "Semester" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentProjects");
        }
    }
}
