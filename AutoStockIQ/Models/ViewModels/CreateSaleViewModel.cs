using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.ViewModels;

public class CreateSaleViewModel
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Customer name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Customer type")]
    public string CustomerType { get; set; } = "School"; // "School" or "Business"

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<SaleLineItem> Items { get; set; } = new();
}

public class SaleLineItem
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}