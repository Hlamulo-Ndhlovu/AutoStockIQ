using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.ViewModels;

public class PlaceSchoolOrderViewModel
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 10_000)]
    public int Quantity { get; set; } = 1;
}
