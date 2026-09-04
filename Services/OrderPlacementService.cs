using AutoStockIQ.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Services;

public class OrderPlacementResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public int? OrderId { get; init; }
    public bool RefillAlertRaised { get; init; }
}

public class OrderBatchResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<int> OrderIds { get; init; } = Array.Empty<int>();
    public bool RefillAlertRaised { get; init; }
}

public class OrderPlacementService
{
    private readonly ApplicationDbContext _db;

    public OrderPlacementService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<OrderPlacementResult> PlaceSchoolOrderAsync(
        string schoolUserId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 1)
            return new OrderPlacementResult { Success = false, Error = "Quantity must be at least 1." };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var inner = await PlaceOneOrderCoreAsync(schoolUserId, productId, quantity, cancellationToken);
            if (!inner.Success)
            {
                await tx.RollbackAsync(cancellationToken);
                return inner;
            }

            await tx.CommitAsync(cancellationToken);
            return inner;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrderBatchResult> PlaceSchoolOrdersBatchAsync(
        string schoolUserId,
        IReadOnlyList<(int ProductId, int Quantity)> lines,
        CancellationToken cancellationToken = default)
    {
        if (lines.Count == 0)
            return new OrderBatchResult { Success = false, Error = "No line items." };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var orderIds = new List<int>();
            var anyRefill = false;
            foreach (var (productId, quantity) in lines)
            {
                var one = await PlaceOneOrderCoreAsync(schoolUserId, productId, quantity, cancellationToken);
                if (!one.Success || one.OrderId is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return new OrderBatchResult { Success = false, Error = one.Error ?? "Order failed." };
                }

                orderIds.Add(one.OrderId.Value);
                if (one.RefillAlertRaised)
                    anyRefill = true;
            }

            await tx.CommitAsync(cancellationToken);
            return new OrderBatchResult
            {
                Success = true,
                OrderIds = orderIds,
                RefillAlertRaised = anyRefill,
            };
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<OrderPlacementResult> PlaceOneOrderCoreAsync(
        string schoolUserId,
        int productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        if (quantity < 1)
            return new OrderPlacementResult { Success = false, Error = "Quantity must be at least 1." };

        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
            return new OrderPlacementResult { Success = false, Error = "Product not found." };

        if (product.StockQuantity < quantity)
            return new OrderPlacementResult { Success = false, Error = $"Only {product.StockQuantity} in stock." };

        product.StockQuantity -= quantity;

        var order = new SchoolOrder
        {
            SchoolUserId = schoolUserId,
            CreatedAtUtc = DateTime.UtcNow,
            Status = SchoolOrderStatus.Pending,
        };
        _db.SchoolOrders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        _db.OrderLines.Add(new OrderLine
        {
            SchoolOrderId = order.Id,
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = product.UnitPrice,
        });

        var refillRaised = false;
        if (product.StockQuantity <= product.ReorderLevel)
        {
            var msg =
                $"Low stock: \"{product.Name}\" ({product.Sku}) is now {product.StockQuantity} on hand " +
                $"(reorder level {product.ReorderLevel}). Administrators are notified; Sales should plan a refill.";

            _db.StockRefillAlerts.Add(new StockRefillAlert
            {
                ProductId = product.Id,
                QuantityAfter = product.StockQuantity,
                ReorderLevel = product.ReorderLevel,
                Message = msg,
                CreatedAtUtc = DateTime.UtcNow,
                TriggeredBySchoolUserId = schoolUserId,
            });
            refillRaised = true;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new OrderPlacementResult
        {
            Success = true,
            OrderId = order.Id,
            RefillAlertRaised = refillRaised,
        };
    }
}
