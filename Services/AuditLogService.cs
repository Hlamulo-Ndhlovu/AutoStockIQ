using AutoStockIQ.Data;
using System.Text.Json;

namespace AutoStockIQ.Services;

public class AuditLogService
{
    private readonly FirestoreService _firestoreService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(FirestoreService firestoreService, IHttpContextAccessor httpContextAccessor)
    {
        _firestoreService = firestoreService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string userId, string userName, string userEmail, string actionType, string entityType, string entityId, string description, object? details = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

        var auditLog = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            UserEmail = userEmail,
            IpAddress = ipAddress,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            Details = details != null ? JsonSerializer.Serialize(details) : null
        };

        await _firestoreService.CreateAuditLogAsync(auditLog);
    }

    public async Task LogStockMovementAsync(string userId, string userName, string userEmail, int productId, string productSku, int quantityChange, string reason)
    {
        var details = new
        {
            ProductId = productId,
            ProductSku = productSku,
            QuantityChange = quantityChange,
            Reason = reason
        };

        await LogAsync(
            userId,
            userName,
            userEmail,
            "StockMovement",
            "Product",
            productId.ToString(),
            $"Stock {(quantityChange > 0 ? "increased" : "decreased")} by {Math.Abs(quantityChange)} for product {productSku}",
            details
        );
    }

    public async Task LogOrderCreatedAsync(string userId, string userName, string userEmail, string orderNumber, string orderType)
    {
        await LogAsync(
            userId,
            userName,
            userEmail,
            "OrderCreated",
            orderType,
            orderNumber,
            $"{orderType} {orderNumber} created"
        );
    }

    public async Task LogOrderApprovedAsync(string userId, string userName, string userEmail, string orderNumber, string orderType)
    {
        await LogAsync(
            userId,
            userName,
            userEmail,
            "OrderApproved",
            orderType,
            orderNumber,
            $"{orderType} {orderNumber} approved"
        );
    }

    public async Task LogOrderFulfilledAsync(string userId, string userName, string userEmail, string orderNumber, string orderType)
    {
        await LogAsync(
            userId,
            userName,
            userEmail,
            "OrderFulfilled",
            orderType,
            orderNumber,
            $"{orderType} {orderNumber} fulfilled"
        );
    }

    public async Task LogSaleAsync(string userId, string userName, string userEmail, string saleNumber, decimal totalValue)
    {
        var details = new
        {
            SaleNumber = saleNumber,
            TotalValue = totalValue
        };

        await LogAsync(
            userId,
            userName,
            userEmail,
            "Sale",
            "Sale",
            saleNumber,
            $"Sale {saleNumber} completed with total value {totalValue:C}",
            details
        );
    }

    public async Task LogLoginAsync(string userId, string userName, string userEmail)
    {
        await LogAsync(
            userId,
            userName,
            userEmail,
            "Login",
            "User",
            userId,
            $"User {userName} logged in"
        );
    }

    public async Task<List<AuditLog>> GetFilteredAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? userId = null, string? actionType = null)
    {
        return await _firestoreService.GetAuditLogsAsync(startDate, endDate, userId, actionType);
    }
}
