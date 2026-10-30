using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Services;

public static class LineDetails
{
    // A line is either picked from the stored items or typed in by hand. Whatever the user typed wins;
    // a picked item only fills in what was left blank. Typed-in lines need at least a product name.
    public static async Task<(Item? item, string description, string hsn, ItemUnit unit)> ResolveAsync(
        AppDbContext db, int? itemId, string? description, string? hsn, ItemUnit? unit)
    {
        Item? item = null;
        if (itemId.HasValue)
        {
            item = await db.Items.FindAsync(itemId.Value)
                ?? throw new InvalidOperationException($"Item {itemId} not found");
        }

        var name = string.IsNullOrWhiteSpace(description) ? item?.Name : description.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Enter a product name for every item.");
        }

        var hsnCode = string.IsNullOrWhiteSpace(hsn) ? item?.HsnSacCode ?? string.Empty : hsn.Trim();
        return (item, name, hsnCode, unit ?? item?.Unit ?? ItemUnit.Nos);
    }
}
