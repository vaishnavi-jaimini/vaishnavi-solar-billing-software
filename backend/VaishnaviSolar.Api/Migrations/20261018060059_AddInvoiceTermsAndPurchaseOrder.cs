using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaishnaviSolar.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceTermsAndPurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DueDate",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "As per Agreement");

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderNo",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Terms",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Due on Receipt");

            migrationBuilder.AddColumn<string>(
                name: "WorkOrderNo",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderNo",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Terms",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WorkOrderNo",
                table: "Invoices");
        }
    }
}
