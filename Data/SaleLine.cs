namespace AutoStockIQ.Data;

public class SaleLine
{
    public string Id { get; set; } = string.Empty;
    
    public string SaleId { get; set; } = string.Empty;
    
    public int ProductId { get; set; }
    
    public string ProductSku { get; set; } = string.Empty;
    
    public string ProductName { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal LineTotal { get; set; }
}
