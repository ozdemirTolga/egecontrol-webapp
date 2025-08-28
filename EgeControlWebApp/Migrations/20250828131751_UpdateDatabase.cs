using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgeControlWebApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CreatedByUserId",
                table: "Quotes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_LastModifiedByUserId",
                table: "Quotes",
                column: "LastModifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_AspNetUsers_CreatedByUserId",
                table: "Quotes",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_AspNetUsers_LastModifiedByUserId",
                table: "Quotes",
                column: "LastModifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_AspNetUsers_CreatedByUserId",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_AspNetUsers_LastModifiedByUserId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_CreatedByUserId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_LastModifiedByUserId",
                table: "Quotes");
        }
    }
}
