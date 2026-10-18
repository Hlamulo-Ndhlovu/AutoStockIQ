using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.ViewModels;

public class AuditLogFilterViewModel
{
    public string? UserId { get; set; }
    
    public string? ActionType { get; set; }
    
    public string? EntityType { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}