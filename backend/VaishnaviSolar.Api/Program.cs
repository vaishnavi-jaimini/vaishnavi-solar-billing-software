using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Render (and many other PaaS hosts) inject a PORT env var the app must bind to.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Render's managed Postgres exposes a DATABASE_URL in URI form (postgres://user:pass@host:port/db).
// When present we run against Postgres; otherwise we fall back to local SQL Server (LocalDB) for dev.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var usePostgres = !string.IsNullOrWhiteSpace(databaseUrl);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (usePostgres)
    {
        options.UseNpgsql(ToNpgsqlConnectionString(databaseUrl!));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<QuotationService>();
builder.Services.AddScoped<ProformaInvoiceService>();

var frontendOrigin = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var origins = new List<string> { "http://localhost:4200" };
        if (!string.IsNullOrWhiteSpace(frontendOrigin))
        {
            origins.Add(frontendOrigin);
        }
        policy.WithOrigins(origins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (usePostgres)
    {
        // Log only the host (never credentials) so a stale or mistyped DATABASE_URL is easy to spot in Render's logs.
        app.Logger.LogInformation("Connecting to PostgreSQL host {Host}", new Uri(databaseUrl!).Host);
        // No Postgres migration set is maintained; build the schema straight from the current model.
        db.Database.EnsureCreated();
        // EnsureCreated never alters an existing schema, so columns added after the first deploy are patched in here.
        db.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Companies\" ADD COLUMN IF NOT EXISTS \"BankAccountName\" text NOT NULL DEFAULT ''; " +
            "ALTER TABLE \"Companies\" ADD COLUMN IF NOT EXISTS \"BankBranch\" text NOT NULL DEFAULT ''; " +
            "ALTER TABLE \"Invoices\" ADD COLUMN IF NOT EXISTS \"Terms\" text NOT NULL DEFAULT 'Due on Receipt'; " +
            "ALTER TABLE \"Invoices\" ADD COLUMN IF NOT EXISTS \"DueDate\" text NOT NULL DEFAULT 'As per Agreement'; " +
            "ALTER TABLE \"Invoices\" ADD COLUMN IF NOT EXISTS \"WorkOrderNo\" text NOT NULL DEFAULT ''; " +
            "ALTER TABLE \"Invoices\" ADD COLUMN IF NOT EXISTS \"PurchaseOrderNo\" text NOT NULL DEFAULT ''; " +
            "ALTER TABLE \"Invoices\" ADD COLUMN IF NOT EXISTS \"PurchaseDate\" timestamp with time zone NULL; " +
            "ALTER TABLE \"Quotations\" ADD COLUMN IF NOT EXISTS \"TermsAndConditions\" text NOT NULL DEFAULT ''; " +
            "ALTER TABLE \"Companies\" ADD COLUMN IF NOT EXISTS \"QuotationPrefix\" text NOT NULL DEFAULT 'VS/SL###/2026-27'; " +
            // The quotation counter continues after the quotations already saved, so it is only seeded when first added.
            "DO $$ BEGIN " +
            "IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Companies' AND column_name = 'NextQuotationNumber') THEN " +
            "ALTER TABLE \"Companies\" ADD COLUMN \"NextQuotationNumber\" integer NOT NULL DEFAULT 1; " +
            "UPDATE \"Companies\" SET \"NextQuotationNumber\" = (SELECT COUNT(*) FROM \"Quotations\") + 1; " +
            "END IF; END $$; " +
            // Line items can be typed in by hand, so they no longer need a stored item.
            "ALTER TABLE \"QuotationItems\" ALTER COLUMN \"ItemId\" DROP NOT NULL; " +
            "ALTER TABLE \"ProformaInvoiceItems\" ALTER COLUMN \"ItemId\" DROP NOT NULL; " +
            "ALTER TABLE \"InvoiceItems\" ALTER COLUMN \"ItemId\" DROP NOT NULL; " +
            // Quotation lines now keep their own HSN and unit; existing lines copy them from their item once.
            "DO $$ BEGIN " +
            "IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationItems' AND column_name = 'HsnSacCode') THEN " +
            "ALTER TABLE \"QuotationItems\" ADD COLUMN \"HsnSacCode\" text NOT NULL DEFAULT ''; " +
            "ALTER TABLE \"QuotationItems\" ADD COLUMN \"Unit\" integer NOT NULL DEFAULT 0; " +
            "UPDATE \"QuotationItems\" qi SET \"HsnSacCode\" = i.\"HsnSacCode\", \"Unit\" = i.\"Unit\" FROM \"Items\" i WHERE i.\"Id\" = qi.\"ItemId\"; " +
            "END IF; END $$;");
    }
    else
    {
        db.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

static string ToNpgsqlConnectionString(string databaseUrl)
{
    // Convert postgres://user:pass@host:port/db?sslmode=require into Npgsql's key=value format.
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require
    };
    return builder.ConnectionString;
}
