using System.Security.Claims;
using AutoStockIQ.Data;
using AutoStockIQ.Models.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers.Api;

[ApiController]
[Route("api/company")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = AuthConstants.CompanyStaffRoles)]
[EnableRateLimiting("api-tight")]
public class CompanyApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public CompanyApiController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    private string? Persona => User.FindFirstValue(ApiClaimTypes.CompanyPersona);

    [HttpGet("dashboard")]
    public async Task<ActionResult<CompanyDashboardResponse>> Dashboard(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null || string.IsNullOrEmpty(Persona))
            return Unauthorized(new { error = "Missing company persona in token. Sign in again as staff." });

        if (Persona == CompanyPortalSession.PersonaAdmin
            && !await _userManager.IsInRoleAsync(user, AuthConstants.CompanyAdminRole))
            return Forbid();

        if (Persona == CompanyPortalSession.PersonaSales
            && !await _userManager.IsInRoleAsync(user, AuthConstants.CompanySalesRole))
            return Forbid();

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var alerts = await _db.StockRefillAlerts.AsNoTracking()
            .Include(a => a.Product)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(30)
            .Select(a => new StockRefillAlertDto
            {
                Id = a.Id,
                CreatedAtUtc = a.CreatedAtUtc,
                ProductId = a.ProductId,
                ProductName = a.Product != null ? a.Product.Name : null,
                ProductSku = a.Product != null ? a.Product.Sku : null,
                QuantityAfter = a.QuantityAfter,
                ReorderLevel = a.ReorderLevel,
                Message = a.Message,
            })
            .ToListAsync(cancellationToken);

        var refillCount = await _db.StockRefillAlerts.AsNoTracking().CountAsync(cancellationToken);

        var statPending = await _db.SchoolOrders.AsNoTracking().CountAsync(o => o.Status == SchoolOrderStatus.Submitted, cancellationToken);
        var statApproved = await _db.SchoolOrders.AsNoTracking().CountAsync(o => o.Status == SchoolOrderStatus.Approved, cancellationToken);
        var statFulfilled = await _db.SchoolOrders.AsNoTracking().CountAsync(o => o.Status == SchoolOrderStatus.Fulfilled, cancellationToken);
        var statCancelledMonth = await _db.SchoolOrders.AsNoTracking().CountAsync(o =>
            o.Status == SchoolOrderStatus.Cancelled && o.ApprovedAtUtc >= monthStart, cancellationToken);

        var oldestPendingUtc = await _db.SchoolOrders.AsNoTracking()
            .Where(o => o.Status == SchoolOrderStatus.Submitted)
            .OrderBy(o => o.CreatedAtUtc)
            .Select(o => (DateTime?)o.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var oldestLabel = oldestPendingUtc is { } t
            ? $"{(DateTime.UtcNow - t).TotalHours:0} h waiting"
            : "—";

        var products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
        var stockValuation = products.Sum(p => p.StockQuantity * p.UnitPrice);

        IReadOnlyList<PendingOrderDto>? pendingOrders = null;
        IReadOnlyList<DecidedOrderDto>? recent = null;
        IReadOnlyList<ProductDto>? productsForRefill = null;

        if (Persona == CompanyPortalSession.PersonaAdmin)
        {
            var rawPending = await _db.SchoolOrders.AsNoTracking()
                .Include(o => o.SchoolUser)
                .Include(o => o.Lines).ThenInclude(l => l.Product)
                .Where(o => o.Status == SchoolOrderStatus.Submitted)
                .OrderBy(o => o.CreatedAtUtc)
                .ToListAsync(cancellationToken);
            pendingOrders = rawPending.Select(o => new PendingOrderDto
            {
                Id = o.Id,
                CreatedAtUtc = o.CreatedAtUtc,
                SchoolEmail = o.SchoolUser?.Email,
                Lines = o.Lines.Select(l => new OrderLineDto
                {
                    ProductId = l.ProductId,
                    ProductSku = l.Product?.Sku,
                    ProductName = l.Product?.Name,
                    Quantity = l.Quantity,
                }).ToList(),
            }).ToList();

            var rawDecided = await _db.SchoolOrders.AsNoTracking()
                .Include(o => o.SchoolUser)
                .Where(o => o.Status != SchoolOrderStatus.Submitted)
                .OrderByDescending(o => o.ApprovedAtUtc)
                .Take(12)
                .ToListAsync(cancellationToken);
            recent = rawDecided.Select(o => new DecidedOrderDto
            {
                Id = o.Id,
                Status = o.Status.ToString(),
                DecidedAtUtc = o.DecidedAtUtc,
                SchoolEmail = o.SchoolUser?.Email,
                DecisionNote = o.DecisionNote,
            }).ToList();
        }

        if (Persona == CompanyPortalSession.PersonaSales)
        {
            productsForRefill = await _db.Products.AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    Category = p.Category,
                    StockQuantity = p.StockQuantity,
                    ReorderLevel = p.ReorderLevel,
                    UnitPrice = p.UnitPrice,
                    SupplierId = p.SupplierId,
                    Location = p.Location,
                    IsActive = p.IsActive,
                })
                .ToListAsync(cancellationToken);
        }

        return Ok(new CompanyDashboardResponse
        {
            Persona = Persona,
            Email = user.Email ?? "",
            StatPending = statPending,
            StatApproved = statApproved,
            StatFulfilled = statFulfilled,
            StatCancelledThisMonth = statCancelledMonth,
            RefillAlertCount = refillCount,
            OldestPendingLabel = oldestLabel,
            StockValuation = stockValuation,
            Alerts = alerts,
            PendingOrders = pendingOrders,
            RecentDecisions = recent,
            ProductsForRefill = productsForRefill,
        });
    }

    [HttpPost("orders/{orderId:int}/approve")]
    public async Task<IActionResult> Approve(int orderId, CancellationToken cancellationToken)
    {
        if (Persona != CompanyPortalSession.PersonaAdmin)
            return Forbid();

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        var order = await _db.SchoolOrders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null || order.Status != SchoolOrderStatus.Submitted)
        {
            await tx.RollbackAsync(cancellationToken);
            return BadRequest(new { error = "Order not found or not in submitted status." });
        }

        order.Status = SchoolOrderStatus.Approved;
        order.ApprovedAtUtc = DateTime.UtcNow;
        order.ApprovedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        order.ApprovedByUserName = User.Identity?.Name ?? "";
        order.DecisionNote = null;
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return Ok(new { message = $"Order {order.OrderNumber} approved." });
    }

    [HttpPost("orders/{orderId:int}/reject")]
    public async Task<IActionResult> Reject(int orderId, [FromBody] RejectOrderRequest? body, CancellationToken cancellationToken)
    {
        if (Persona != CompanyPortalSession.PersonaAdmin)
            return Forbid();

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        var order = await _db.SchoolOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null || order.Status != SchoolOrderStatus.Submitted)
        {
            await tx.RollbackAsync(cancellationToken);
            return BadRequest(new { error = "Order not found or not in submitted status." });
        }

        var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);
        foreach (var line in order.Lines)
        {
            if (products.TryGetValue(line.ProductId, out var product))
                product.StockQuantity += line.Quantity;
        }

        foreach (var pid in productIds)
        {
            if (products.TryGetValue(pid, out var p) && p.StockQuantity > p.ReorderLevel)
                await _db.StockRefillAlerts.Where(a => a.ProductId == pid).ExecuteDeleteAsync(cancellationToken);
        }

        order.Status = SchoolOrderStatus.Cancelled;
        order.ApprovedAtUtc = DateTime.UtcNow;
        order.ApprovedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        order.ApprovedByUserName = User.Identity?.Name ?? "";
        order.DecisionNote = string.IsNullOrWhiteSpace(body?.Note) ? null : body.Note.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return Ok(new { message = $"Order {order.OrderNumber} cancelled; stock restored." });
    }

    [HttpPost("orders/{orderId:int}/fulfill")]
    public async Task<IActionResult> Fulfill(int orderId, CancellationToken cancellationToken)
    {
        if (Persona != CompanyPortalSession.PersonaAdmin)
            return Forbid();

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        var order = await _db.SchoolOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null || order.Status != SchoolOrderStatus.Approved)
        {
            await tx.RollbackAsync(cancellationToken);
            return BadRequest(new { error = "Order not found or not approved." });
        }

        var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);
        
        foreach (var line in order.Lines)
        {
            if (products.TryGetValue(line.ProductId, out var product))
            {
                if (product.StockQuantity < line.Quantity)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return BadRequest(new { error = $"Insufficient stock for {product.Name}" });
                }
                product.StockQuantity -= line.Quantity;
            }
        }

        order.Status = SchoolOrderStatus.Fulfilled;
        order.FulfilledAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return Ok(new { message = $"Order {order.OrderNumber} fulfilled." });
    }

    [HttpPost("stock/refill")]
    public async Task<IActionResult> Refill([FromBody] RefillStockRequest model, CancellationToken cancellationToken)
    {
        if (Persona != CompanyPortalSession.PersonaSales)
            return Forbid();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var product = await _db.Products.FindAsync(new object[] { model.ProductId }, cancellationToken);
        if (product is null)
            return BadRequest(new { error = "Product not found." });

        product.StockQuantity += model.QuantityAdded;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new
        {
            message = $"Added {model.QuantityAdded} units to \"{product.Name}\". On-hand is now {product.StockQuantity}.",
        });
    }
}
