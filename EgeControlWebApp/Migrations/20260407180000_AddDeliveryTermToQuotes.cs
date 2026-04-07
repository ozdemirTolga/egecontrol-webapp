using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgeControlWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryTermToQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryTerm",
                table: "Quotes",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryTerm",
                table: "Quotes");
        }
    }
}
