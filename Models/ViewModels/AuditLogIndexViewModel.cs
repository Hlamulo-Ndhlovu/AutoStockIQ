using AutoStockIQ.Data;

namespace AutoStockIQ.Models.ViewModels;

public class AuditLogIndexViewModel
{
    public AuditLogFilterViewModel Filter { get; set; } = new();
    
    public List<AuditLog> AuditLogs { get; set; } = new();
}