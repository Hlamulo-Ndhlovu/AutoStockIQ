namespace AutoStockIQ.Data;

public class OrderLine
{
    public int Id { get; set; }

    public int SchoolOrderId { get; set; }

    public SchoolOrder Order { get; set; } = null!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}
