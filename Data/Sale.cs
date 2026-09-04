namespace AutoStockIQ.Data;

public class Sale
{
    public string Id { get; set; } = string.Empty;
    
    public string SaleNumber { get; set; } = string.Empty;
    
    public string CustomerId { get; set; } = string.Empty;
    
    public string CustomerName { get; set; } = string.Empty;
    
    public string CustomerType { get; set; } = string.Empty; // "School" or "Business"
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    public decimal TotalValue { get; set; }
    
    public string ProcessedByUserId { get; set; } = string.Empty;
    
    public string ProcessedByUserName { get; set; } = string.Empty;
    
    public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
}
