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
        // No Postgres migration set is maintained; build the schema straight from the current model.
        db.Database.EnsureCreated();

        // TEMPORARY: one-time removal of test data (see RemoveTestData migration). Idempotent; delete once it has run.
        using var tx = db.Database.BeginTransaction();
        foreach (var sql in VaishnaviSolar.Api.Migrations.RemoveTestData.Statements())
        {
            db.Database.ExecuteSqlRaw(sql);
        }
        tx.Commit();
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
