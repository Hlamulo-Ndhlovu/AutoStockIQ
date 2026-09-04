using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.ViewModels;

public class CompanyLoginViewModel
{
    [Required(ErrorMessage = "Choose whether you are signing in as Administrator or Sales.")]
    [RegularExpression("^(Admin|Sales)$", ErrorMessage = "Select Administrator or Sales.")]
    [Display(Name = "Role")]
    public string Persona { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
