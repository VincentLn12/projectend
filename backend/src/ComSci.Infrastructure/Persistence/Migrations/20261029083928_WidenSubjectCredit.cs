using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WidenSubjectCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Credit",
                table: "Subjects",
                type: "decimal(6,1)",
                precision: 6,
                scale: 1,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Credit",
                table: "Subjects",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,1)",
                oldPrecision: 6,
                oldScale: 1);
        }
    }
}
