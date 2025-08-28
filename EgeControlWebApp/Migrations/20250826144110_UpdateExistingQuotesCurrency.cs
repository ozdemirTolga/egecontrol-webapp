using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgeControlWebApp.Migrations
{
    /// <summary>
    /// Obsolete duplicate migration kept only for history; renamed to avoid class collision.
    /// </summary>
    public partial class UpdateExistingQuotesCurrency_Obsolete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing quotes to have EUR currency if they don't have one
            migrationBuilder.Sql("UPDATE Quotes SET Currency = 'EUR' WHERE Currency IS NULL OR Currency = '';");
        }

        /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to empty currency
            migrationBuilder.Sql("UPDATE Quotes SET Currency = '' WHERE Currency = 'EUR';");
        }
    }
}
