using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaishnaviSolar.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationNumbering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NextQuotationNumber",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuotationPrefix",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NextQuotationNumber", "QuotationPrefix" },
                values: new object[] { 1, "VS/SL###/2026-27" });

            // Continue the counter after the quotations already saved.
            migrationBuilder.Sql("UPDATE Companies SET NextQuotationNumber = (SELECT COUNT(*) FROM Quotations) + 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextQuotationNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "QuotationPrefix",
                table: "Companies");
        }
    }
}
