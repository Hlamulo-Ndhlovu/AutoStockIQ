namespace AutoStockIQ.Data;

public class AuditLog
{
    public string Id { get; set; } = string.Empty;
    
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    public string UserId { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;
    
    public string UserEmail { get; set; } = string.Empty;
    
    public string IpAddress { get; set; } = string.Empty;
    
    public string ActionType { get; set; } = string.Empty; // "StockMovement", "OrderCreated", "OrderApproved", "OrderFulfilled", "Sale", "Login", etc.
    
    public string EntityType { get; set; } = string.Empty; // "Product", "Order", "Sale", "User", etc.
    
    public string EntityId { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string? Details { get; set; } // JSON string for additional details
}
