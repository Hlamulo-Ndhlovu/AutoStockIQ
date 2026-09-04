namespace AutoStockIQ.Data;

public enum SchoolOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Fulfilled = 3,
    Cancelled = 4,

    // Legacy values for backward compatibility
    Pending = Submitted,
    Accepted = Approved,
    Rejected = Cancelled
}

