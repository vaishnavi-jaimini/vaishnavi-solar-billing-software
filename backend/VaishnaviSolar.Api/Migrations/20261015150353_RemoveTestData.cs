using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaishnaviSolar.Api.Migrations
{
    /// <summary>
    /// One-time cleanup of the "ZZ TEST" records (and the "Test" Kerala customer) created while testing
    /// the deployed app. SQL uses double-quoted identifiers so it runs on both PostgreSQL and SQL Server.
    /// A JSON backup of the live data was taken beforehand; this migration cannot be reverted.
    /// </summary>
    public partial class RemoveTestData : Migration
    {
        private const string TestCustomers =
            "SELECT \"Id\" FROM \"Customers\" WHERE \"Name\" LIKE 'ZZ TEST%' OR (\"Name\" = 'Test' AND \"State\" = 'Kerala')";

        private const string TestItems =
            "SELECT \"Id\" FROM \"Items\" WHERE \"Name\" LIKE 'ZZ TEST%'";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var sql in Statements())
            {
                migrationBuilder.Sql(sql);
            }
        }

        /// <summary>
        /// The cleanup statements, in dependency order. Also run directly at startup on PostgreSQL,
        /// which builds its schema with EnsureCreated and never applies migrations.
        /// </summary>
        public static IEnumerable<string> Statements()
        {
            var testInvoices = $"SELECT \"Id\" FROM \"Invoices\" WHERE \"CustomerId\" IN ({TestCustomers})";
            var testQuotations = $"SELECT \"Id\" FROM \"Quotations\" WHERE \"CustomerId\" IN ({TestCustomers})";
            var testProformas = $"SELECT \"Id\" FROM \"ProformaInvoices\" WHERE \"CustomerId\" IN ({TestCustomers})";

            yield return $"DELETE FROM \"Payments\" WHERE \"InvoiceId\" IN ({testInvoices});";
            yield return $"DELETE FROM \"InvoiceItems\" WHERE \"InvoiceId\" IN ({testInvoices});";
            yield return $"DELETE FROM \"Invoices\" WHERE \"Id\" IN ({testInvoices});";

            yield return $"DELETE FROM \"QuotationItems\" WHERE \"QuotationId\" IN ({testQuotations});";
            yield return $"DELETE FROM \"Quotations\" WHERE \"Id\" IN ({testQuotations});";

            yield return $"DELETE FROM \"ProformaInvoiceItems\" WHERE \"ProformaInvoiceId\" IN ({testProformas});";
            yield return $"DELETE FROM \"ProformaInvoices\" WHERE \"Id\" IN ({testProformas});";

            // Only remove test items no longer used on any remaining document (FKs cascade to line items).
            var unusedTestItems = $@"SELECT i.""Id"" FROM ""Items"" i WHERE i.""Id"" IN ({TestItems})
                AND NOT EXISTS (SELECT 1 FROM ""InvoiceItems"" x WHERE x.""ItemId"" = i.""Id"")
                AND NOT EXISTS (SELECT 1 FROM ""QuotationItems"" x WHERE x.""ItemId"" = i.""Id"")
                AND NOT EXISTS (SELECT 1 FROM ""ProformaInvoiceItems"" x WHERE x.""ItemId"" = i.""Id"")";
            yield return $"DELETE FROM \"StockTransactions\" WHERE \"ItemId\" IN ({unusedTestItems});";
            yield return $"DELETE FROM \"Items\" WHERE \"Id\" IN ({unusedTestItems});";

            yield return $@"DELETE FROM ""Customers"" WHERE ""Id"" IN ({TestCustomers})
                AND NOT EXISTS (SELECT 1 FROM ""Invoices"" x WHERE x.""CustomerId"" = ""Customers"".""Id"")
                AND NOT EXISTS (SELECT 1 FROM ""Quotations"" x WHERE x.""CustomerId"" = ""Customers"".""Id"")
                AND NOT EXISTS (SELECT 1 FROM ""ProformaInvoices"" x WHERE x.""CustomerId"" = ""Customers"".""Id"");";

            // Restart numbering only when nothing is left that could collide with a reused number.
            yield return "UPDATE \"Companies\" SET \"NextInvoiceNumber\" = 1 WHERE NOT EXISTS (SELECT 1 FROM \"Invoices\");";
            yield return "UPDATE \"Companies\" SET \"NextProformaNumber\" = 1 WHERE NOT EXISTS (SELECT 1 FROM \"ProformaInvoices\");";
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data deletion is not reversible; restore from the JSON backup if needed.
        }
    }
}
