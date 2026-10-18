using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.ViewModels;

public class ProductViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(64)]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Category")]
    public string Category { get; set; } = "Stationery";

    [Range(0, int.MaxValue)]
    [Display(Name = "Stock quantity")]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Reorder level")]
    public int ReorderLevel { get; set; }

    [Range(0, 9999999)]
    [Display(Name = "Unit price (ZAR)")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "Supplier")]
    public string? SupplierId { get; set; }

    [StringLength(200)]
    [Display(Name = "Location")]
    public string? Location { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
