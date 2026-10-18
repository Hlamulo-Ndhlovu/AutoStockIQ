using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.ViewModels;

public class RefillStockViewModel
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 1_000_000)]
    [Display(Name = "Units to add")]
    public int QuantityAdded { get; set; } = 1;
}
