using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.ViewModels;

public class SchoolRegisterViewModel
{
    [Required]
    [StringLength(200)]
    [Display(Name = "School name")]
    public string SchoolName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the POPIA consent to continue.")]
    [Display(Name = "I consent to the processing of my personal data in accordance with POPIA")]
    public bool PopiaConsent { get; set; }
}
