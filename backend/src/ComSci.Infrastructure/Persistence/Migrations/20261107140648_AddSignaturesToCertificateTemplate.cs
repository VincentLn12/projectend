using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComSci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSignaturesToCertificateTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SecondSignatureFileId",
                table: "CertificateTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SignatureFileId",
                table: "CertificateTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_SecondSignatureFileId",
                table: "CertificateTemplates",
                column: "SecondSignatureFileId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_SignatureFileId",
                table: "CertificateTemplates",
                column: "SignatureFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_CertificateTemplates_Files_SecondSignatureFileId",
                table: "CertificateTemplates",
                column: "SecondSignatureFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CertificateTemplates_Files_SignatureFileId",
                table: "CertificateTemplates",
                column: "SignatureFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CertificateTemplates_Files_SecondSignatureFileId",
                table: "CertificateTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_CertificateTemplates_Files_SignatureFileId",
                table: "CertificateTemplates");

            migrationBuilder.DropIndex(
                name: "IX_CertificateTemplates_SecondSignatureFileId",
                table: "CertificateTemplates");

            migrationBuilder.DropIndex(
                name: "IX_CertificateTemplates_SignatureFileId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "SecondSignatureFileId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "SignatureFileId",
                table: "CertificateTemplates");
        }
    }
}
