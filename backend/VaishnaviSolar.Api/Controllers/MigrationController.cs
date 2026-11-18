using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VaishnaviSolar.Api.Data;

namespace VaishnaviSolar.Api.Controllers;

// TEMPORARY: copies every table row for row (ids included) from one free Render Postgres to the next.
// Remove once the database move is done.
[ApiController]
[Route("api/[controller]")]
public class MigrationController : ControllerBase
{
    private readonly AppDbContext _db;
    public MigrationController(AppDbContext db) => _db = db;

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        if (!_db.Database.IsNpgsql()) return BadRequest("Only available on PostgreSQL.");
        var conn = await OpenAsync();
        var result = new Dictionary<string, JsonElement>();
        foreach (var table in await TablesInInsertOrderAsync(conn))
        {
            var json = (string)(await ScalarAsync(conn, $"SELECT COALESCE(json_agg(t), '[]'::json)::text FROM \"{table}\" t"))!;
            result[table] = JsonDocument.Parse(json).RootElement.Clone();
        }
        return Ok(result);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(Dictionary<string, JsonElement> data)
    {
        if (!_db.Database.IsNpgsql()) return BadRequest("Only available on PostgreSQL.");
        // Only a freshly created database (seed data only) may be overwritten.
        if (await _db.Customers.AnyAsync() || await _db.Invoices.AnyAsync() || await _db.Quotations.AnyAsync())
            return Conflict("This database already holds data; import refused.");

        var conn = await OpenAsync();
        var tables = await TablesInInsertOrderAsync(conn);
        var missing = data.Keys.Except(tables).ToList();
        if (missing.Count > 0) return BadRequest($"Unknown tables: {string.Join(", ", missing)}");

        await using var tx = await conn.BeginTransactionAsync();
        await ExecAsync(conn, $"TRUNCATE {string.Join(", ", tables.Select(t => $"\"{t}\""))} RESTART IDENTITY CASCADE", tx);

        var counts = new Dictionary<string, long>();
        foreach (var table in tables.Where(data.ContainsKey))
        {
            await using var cmd = new NpgsqlCommand(
                $"INSERT INTO \"{table}\" SELECT * FROM json_populate_recordset(NULL::\"{table}\", @rows::json)", (NpgsqlConnection)conn, (NpgsqlTransaction)tx);
            cmd.Parameters.AddWithValue("rows", data[table].GetRawText());
            counts[table] = await cmd.ExecuteNonQueryAsync();

            // Move each identity sequence past the copied ids so new records don't collide.
            await ExecAsync(conn,
                $"SELECT setval(pg_get_serial_sequence('\"{table}\"', 'Id'), COALESCE((SELECT MAX(\"Id\") FROM \"{table}\"), 0) + 1, false) " +
                $"WHERE pg_get_serial_sequence('\"{table}\"', 'Id') IS NOT NULL", tx);
        }
        await tx.CommitAsync();
        return Ok(counts);
    }

    private async Task<DbConnection> OpenAsync()
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        return conn;
    }

    // Tables ordered so every table comes after the tables its foreign keys point to.
    private static async Task<List<string>> TablesInInsertOrderAsync(DbConnection conn)
    {
        var tables = new List<string>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE' ORDER BY table_name";
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) tables.Add(r.GetString(0));
        }

        var deps = tables.ToDictionary(t => t, _ => new HashSet<string>());
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText =
                "SELECT tc.table_name, ccu.table_name FROM information_schema.table_constraints tc " +
                "JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name AND tc.table_schema = ccu.table_schema " +
                "WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_schema = 'public'";
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var (child, parent) = (r.GetString(0), r.GetString(1));
                if (child != parent && deps.ContainsKey(child)) deps[child].Add(parent);
            }
        }

        var ordered = new List<string>();
        while (deps.Count > 0)
        {
            var ready = deps.Where(d => d.Value.All(ordered.Contains)).Select(d => d.Key).OrderBy(t => t).ToList();
            if (ready.Count == 0) throw new InvalidOperationException("Circular foreign keys between tables.");
            foreach (var t in ready) { ordered.Add(t); deps.Remove(t); }
        }
        return ordered;
    }

    private static async Task<object?> ScalarAsync(DbConnection conn, string sql)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return await cmd.ExecuteScalarAsync();
    }

    private static async Task ExecAsync(DbConnection conn, string sql, DbTransaction? tx = null)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        await cmd.ExecuteNonQueryAsync();
    }
}
