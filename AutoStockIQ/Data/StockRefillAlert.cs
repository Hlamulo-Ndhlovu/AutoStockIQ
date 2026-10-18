namespace AutoStockIQ.Data;

/// <summary>Raised when a school order reduces stock to at or below the product reorder level — visible to Admin and Sales.</summary>
public class StockRefillAlert
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int QuantityAfter { get; set; }

    public int ReorderLevel { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? TriggeredBySchoolUserId { get; set; }
}
