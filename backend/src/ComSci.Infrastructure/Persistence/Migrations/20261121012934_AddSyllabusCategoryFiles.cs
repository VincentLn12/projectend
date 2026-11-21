using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSyllabusCategoryFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyllabusCategoryFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectCategory = table.Column<byte>(type: "tinyint", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusCategoryFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyllabusCategoryFiles_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyllabusCategoryFiles_Syllabuses_SyllabusId",
                        column: x => x.SyllabusId,
                        principalTable: "Syllabuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusCategoryFiles_FileId",
                table: "SyllabusCategoryFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusCategoryFiles_SyllabusId_SubjectCategory",
                table: "SyllabusCategoryFiles",
                columns: new[] { "SyllabusId", "SubjectCategory" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyllabusCategoryFiles");
        }
    }
}
