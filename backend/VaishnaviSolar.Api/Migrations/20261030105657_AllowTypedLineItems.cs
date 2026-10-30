using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaishnaviSolar.Api.Migrations
{
    /// <inheritdoc />
    public partial class AllowTypedLineItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_Items_ItemId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProformaInvoiceItems_Items_ItemId",
                table: "ProformaInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItems_Items_ItemId",
                table: "QuotationItems");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "QuotationItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "HsnSacCode",
                table: "QuotationItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "QuotationItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Quotation lines used to read HSN and unit from the item; copy them over so old quotations print the same.
            migrationBuilder.Sql(
                "UPDATE qi SET qi.HsnSacCode = i.HsnSacCode, qi.Unit = i.Unit " +
                "FROM QuotationItems qi JOIN Items i ON i.Id = qi.ItemId");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "ProformaInvoiceItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "InvoiceItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_Items_ItemId",
                table: "InvoiceItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProformaInvoiceItems_Items_ItemId",
                table: "ProformaInvoiceItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItems_Items_ItemId",
                table: "QuotationItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_Items_ItemId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProformaInvoiceItems_Items_ItemId",
                table: "ProformaInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItems_Items_ItemId",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "HsnSacCode",
                table: "QuotationItems");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "QuotationItems");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "QuotationItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "ProformaInvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "InvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_Items_ItemId",
                table: "InvoiceItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProformaInvoiceItems_Items_ItemId",
                table: "ProformaInvoiceItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItems_Items_ItemId",
                table: "QuotationItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
