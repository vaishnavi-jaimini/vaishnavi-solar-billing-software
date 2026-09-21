using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProformaInvoice> ProformaInvoices => Set<ProformaInvoice>();
    public DbSet<ProformaInvoiceItem> ProformaInvoiceItems => Set<ProformaInvoiceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>()
            .Property(i => i.Rate).HasPrecision(18, 2);
        modelBuilder.Entity<Item>()
            .Property(i => i.GstPercent).HasPrecision(5, 2);
        modelBuilder.Entity<Item>()
            .Property(i => i.CurrentStock).HasPrecision(18, 3);
        modelBuilder.Entity<Item>()
            .Property(i => i.LowStockThreshold).HasPrecision(18, 3);

        modelBuilder.Entity<StockTransaction>()
            .Property(s => s.Quantity).HasPrecision(18, 3);

        modelBuilder.Entity<QuotationItem>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 3);
            e.Property(x => x.Rate).HasPrecision(18, 2);
            e.Property(x => x.GstPercent).HasPrecision(5, 2);
            e.Property(x => x.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Quotation>(e =>
        {
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.Property(x => x.TotalGst).HasPrecision(18, 2);
            e.Property(x => x.GrandTotal).HasPrecision(18, 2);
            e.HasOne(x => x.Customer).WithMany(c => c.Quotations).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceItem>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 3);
            e.Property(x => x.Rate).HasPrecision(18, 2);
            e.Property(x => x.GstPercent).HasPrecision(5, 2);
            e.Property(x => x.CgstAmount).HasPrecision(18, 2);
            e.Property(x => x.SgstAmount).HasPrecision(18, 2);
            e.Property(x => x.IgstAmount).HasPrecision(18, 2);
            e.Property(x => x.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Invoice>(e =>
        {
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.Property(x => x.CgstAmount).HasPrecision(18, 2);
            e.Property(x => x.SgstAmount).HasPrecision(18, 2);
            e.Property(x => x.IgstAmount).HasPrecision(18, 2);
            e.Property(x => x.TotalGst).HasPrecision(18, 2);
            e.Property(x => x.GrandTotal).HasPrecision(18, 2);
            e.Property(x => x.AmountPaid).HasPrecision(18, 2);
            e.HasOne(x => x.Customer).WithMany(c => c.Invoices).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
        });

        modelBuilder.Entity<Payment>()
            .Property(x => x.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<ProformaInvoiceItem>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 3);
            e.Property(x => x.Rate).HasPrecision(18, 2);
            e.Property(x => x.GstPercent).HasPrecision(5, 2);
            e.Property(x => x.CgstAmount).HasPrecision(18, 2);
            e.Property(x => x.SgstAmount).HasPrecision(18, 2);
            e.Property(x => x.IgstAmount).HasPrecision(18, 2);
            e.Property(x => x.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ProformaInvoice>(e =>
        {
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.Property(x => x.CgstAmount).HasPrecision(18, 2);
            e.Property(x => x.SgstAmount).HasPrecision(18, 2);
            e.Property(x => x.IgstAmount).HasPrecision(18, 2);
            e.Property(x => x.TotalGst).HasPrecision(18, 2);
            e.Property(x => x.GrandTotal).HasPrecision(18, 2);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.ProformaNumber).IsUnique();
        });

        modelBuilder.Entity<Company>()
            .HasData(new Company
            {
                Id = 1,
                Name = "Vaishnavi Solar",
                Address = "",
                City = "",
                State = "Maharashtra",
                Pincode = "",
                Gstin = "",
                Phone = "",
                Email = "",
                BankName = "",
                BankAccountNo = "",
                BankIfsc = "",
                NextInvoiceNumber = 1,
                InvoicePrefix = "INV-"
            });

        modelBuilder.Entity<Item>().HasData(
            new Item { Id = 1, Name = "Polycrystalline Solar Panel 335W", Category = ItemCategory.Panel, HsnSacCode = "85414011", Unit = ItemUnit.Nos, Rate = 9500, GstPercent = 12, CurrentStock = 0, IsService = false },
            new Item { Id = 2, Name = "Monocrystalline Solar Panel 545W", Category = ItemCategory.Panel, HsnSacCode = "85414011", Unit = ItemUnit.Nos, Rate = 15500, GstPercent = 12, CurrentStock = 0, IsService = false },
            new Item { Id = 3, Name = "On-Grid Solar Inverter 5kW", Category = ItemCategory.Inverter, HsnSacCode = "85044090", Unit = ItemUnit.Nos, Rate = 45000, GstPercent = 12, CurrentStock = 0, IsService = false },
            new Item { Id = 4, Name = "Off-Grid Solar Inverter 3kW", Category = ItemCategory.Inverter, HsnSacCode = "85044090", Unit = ItemUnit.Nos, Rate = 28000, GstPercent = 12, CurrentStock = 0, IsService = false },
            new Item { Id = 5, Name = "Tubular Battery 150Ah", Category = ItemCategory.Battery, HsnSacCode = "85072000", Unit = ItemUnit.Nos, Rate = 16500, GstPercent = 28, CurrentStock = 0, IsService = false },
            new Item { Id = 6, Name = "Lithium Battery 100Ah", Category = ItemCategory.Battery, HsnSacCode = "85076000", Unit = ItemUnit.Nos, Rate = 42000, GstPercent = 18, CurrentStock = 0, IsService = false },
            new Item { Id = 7, Name = "Mounting Structure (per kW)", Category = ItemCategory.MountingStructure, HsnSacCode = "73089090", Unit = ItemUnit.Set, Rate = 3500, GstPercent = 18, CurrentStock = 0, IsService = false },
            new Item { Id = 8, Name = "DC Solar Cable 4mm (Red/Black)", Category = ItemCategory.CableWire, HsnSacCode = "85446090", Unit = ItemUnit.Meter, Rate = 45, GstPercent = 18, CurrentStock = 0, IsService = false },
            new Item { Id = 9, Name = "MC4 Connector Pair", Category = ItemCategory.BalanceOfSystem, HsnSacCode = "85369090", Unit = ItemUnit.Set, Rate = 150, GstPercent = 18, CurrentStock = 0, IsService = false },
            new Item { Id = 10, Name = "Earthing Kit", Category = ItemCategory.BalanceOfSystem, HsnSacCode = "85469090", Unit = ItemUnit.Set, Rate = 1200, GstPercent = 18, CurrentStock = 0, IsService = false },
            new Item { Id = 11, Name = "Solar Installation & Commissioning", Category = ItemCategory.InstallationService, HsnSacCode = "998714", Unit = ItemUnit.Service, Rate = 5000, GstPercent = 18, CurrentStock = 0, IsService = true },
            new Item { Id = 12, Name = "Annual Maintenance Contract (AMC)", Category = ItemCategory.InstallationService, HsnSacCode = "998719", Unit = ItemUnit.Service, Rate = 3000, GstPercent = 18, CurrentStock = 0, IsService = true }
        );
    }
}
