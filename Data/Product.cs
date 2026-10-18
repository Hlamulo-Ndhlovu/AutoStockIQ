namespace AutoStockIQ.Data;

public class Product
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    /// <summary>When on-hand quantity is at or below this value after a school order, Sales and Administrators get a refill alert.</summary>
    public int ReorderLevel { get; set; }

    public decimal UnitPrice { get; set; }

    public string? SupplierId { get; set; }

    public string? Location { get; set; } // Shelf or storeroom location

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? DeactivatedAtUtc { get; set; }
}
