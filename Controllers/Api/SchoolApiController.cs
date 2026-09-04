using System.Security.Claims;
using AutoStockIQ.Data;
using AutoStockIQ.Models.Api;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers.Api;

[ApiController]
[Route("api/school")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = AuthConstants.SchoolRole)]
[EnableRateLimiting("api-tight")]
public class SchoolApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly OrderPlacementService _orders;

    public SchoolApiController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        OrderPlacementService orders)
    {
        _userManager = userManager;
        _db = db;
        _orders = orders;
    }

    [HttpGet("me")]
    public async Task<ActionResult<SchoolSummaryResponse>> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Unauthorized();

        var pending = await _db.SchoolOrders.AsNoTracking()
            .CountAsync(o => o.SchoolUserId == userId && o.Status == SchoolOrderStatus.Pending, cancellationToken);
        var accepted = await _db.SchoolOrders.AsNoTracking()
            .CountAsync(o => o.SchoolUserId == userId && o.Status == SchoolOrderStatus.Accepted, cancellationToken);

        return Ok(new SchoolSummaryResponse
        {
            Email = user.Email ?? "",
            SchoolName = user.SchoolName,
            PendingOrders = pending,
            AcceptedOrders = accepted,
        });
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<SchoolMyOrderDto>>> MyOrders(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var raw = await _db.SchoolOrders.AsNoTracking()
            .Where(o => o.SchoolUserId == userId)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var list = raw.Select(o => new SchoolMyOrderDto
        {
            Id = o.Id,
            CreatedAtUtc = o.CreatedAtUtc,
            Status = o.Status.ToString(),
            DecidedAtUtc = o.DecidedAtUtc,
            DecisionNote = o.DecisionNote,
            Lines = o.Lines.Select(l => new SchoolMyOrderLineDto
            {
                ProductId = l.ProductId,
                ProductName = l.Product?.Name,
                Quantity = l.Quantity,
            }).ToList(),
        }).ToList();

        return Ok(list);
    }

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Products(CancellationToken cancellationToken)
    {
        var list = await _db.Products.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                StockQuantity = p.StockQuantity,
                ReorderLevel = p.ReorderLevel,
                UnitPrice = p.UnitPrice,
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("orders")]
    public async Task<ActionResult<SchoolPlaceOrdersResponse>> PlaceOrders(
        [FromBody] SchoolPlaceOrdersRequest model,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var lines = model.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        var result = await _orders.PlaceSchoolOrdersBatchAsync(userId, lines, cancellationToken);
        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(new SchoolPlaceOrdersResponse
        {
            OrderIds = result.OrderIds,
            RefillAlertRaised = result.RefillAlertRaised,
        });
    }
}
