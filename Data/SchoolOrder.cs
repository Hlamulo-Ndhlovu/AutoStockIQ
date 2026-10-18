namespace AutoStockIQ.Data;

public class SchoolOrder
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string SchoolUserId { get; set; } = string.Empty;

    public ApplicationUser? SchoolUser { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public SchoolOrderStatus Status { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    public DateTime? FulfilledAtUtc { get; set; }

    public string? ApprovedByUserId { get; set; }

    public string? ApprovedByUserName { get; set; }

    public string? DecisionNote { get; set; }

    public decimal TotalValue { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();

    // Legacy property for backward compatibility
    public DateTime? DecidedAtUtc => ApprovedAtUtc;
}
