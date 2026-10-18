using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.ViewModels;

public class SupplierViewModel
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Supplier name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Contact person")]
    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;
}