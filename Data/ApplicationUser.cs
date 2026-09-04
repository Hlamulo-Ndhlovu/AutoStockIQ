using Microsoft.AspNetCore.Identity;

namespace AutoStockIQ.Data;

public class ApplicationUser : IdentityUser
{
    public string? SchoolName { get; set; }
    
    public string? BusinessName { get; set; }
    
    public bool PopiaConsent { get; set; }
    
    public DateTime PopiaConsentDateUtc { get; set; }
    
    public string? PopiaConsentIpAddress { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
