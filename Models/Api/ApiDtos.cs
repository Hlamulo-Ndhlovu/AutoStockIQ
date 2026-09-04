using System.ComponentModel.DataAnnotations;

namespace AutoStockIQ.Models.Api;

public class SchoolRegisterRequest
{
    [Required, StringLength(200)]
    public string SchoolName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the POPIA consent to continue.")]
    public bool PopiaConsent { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class CompanyLoginRequest : LoginRequest
{
    [Required]
    [RegularExpression("^(Admin|Sales)$")]
    public string Persona { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public string? CompanyPersona { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

public class ProductDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public string? SupplierId { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}

public class SchoolSummaryResponse
{
    public string Email { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public int PendingOrders { get; set; }
    public int AcceptedOrders { get; set; }
}

public class SchoolOrderLineRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1_000_000)]
    public int Quantity { get; set; }
}

public class SchoolPlaceOrdersRequest
{
    [Required, MinLength(1)]
    public List<SchoolOrderLineRequest> Items { get; set; } = new();
}

public class SchoolPlaceOrdersResponse
{
    public IReadOnlyList<int> OrderIds { get; set; } = Array.Empty<int>();
    public bool RefillAlertRaised { get; set; }
}

public class SchoolMyOrderLineDto
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
}

public class SchoolMyOrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DecidedAtUtc { get; set; }
    public string? DecisionNote { get; set; }
    public IReadOnlyList<SchoolMyOrderLineDto> Lines { get; set; } = Array.Empty<SchoolMyOrderLineDto>();
}

public class StockRefillAlertDto
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public int QuantityAfter { get; set; }
    public int ReorderLevel { get; set; }
    public string? Message { get; set; }
}

public class PendingOrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? SchoolEmail { get; set; }
    public IReadOnlyList<OrderLineDto> Lines { get; set; } = Array.Empty<OrderLineDto>();
}

public class OrderLineDto
{
    public int ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
}

public class DecidedOrderDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DecidedAtUtc { get; set; }
    public string? SchoolEmail { get; set; }
    public string? DecisionNote { get; set; }
}

public class CompanyDashboardResponse
{
    public string Persona { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int StatPending { get; set; }
    public int StatApproved { get; set; }
    public int StatFulfilled { get; set; }
    public int StatCancelledThisMonth { get; set; }
    public int RefillAlertCount { get; set; }
    public string OldestPendingLabel { get; set; } = "—";
    public decimal StockValuation { get; set; }
    public IReadOnlyList<StockRefillAlertDto> Alerts { get; set; } = Array.Empty<StockRefillAlertDto>();
    public IReadOnlyList<PendingOrderDto>? PendingOrders { get; set; }
    public IReadOnlyList<DecidedOrderDto>? RecentDecisions { get; set; }
    public IReadOnlyList<ProductDto>? ProductsForRefill { get; set; }
}

public class RefillStockRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1_000_000)]
    public int QuantityAdded { get; set; }
}

public class RejectOrderRequest
{
    [StringLength(500)]
    public string? Note { get; set; }
}
