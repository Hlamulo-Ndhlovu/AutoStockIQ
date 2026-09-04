using AutoStockIQ.Data;
using AutoStockIQ.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Route("company")]
public class CompanyController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public CompanyController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }

    [Authorize(Roles = AuthConstants.CompanyStaffRoles)]
    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (!await PortalSessionMatchesUserAsync(user))
        {
            HttpContext.Session.Remove(CompanyPortalSession.PersonaKey);
            TempData["CompanyError"] = "Choose your role and sign in again.";
            return RedirectToAction(nameof(Login));
        }

        ViewBag.Email = user?.Email;
        var persona = HttpContext.Session.GetString(CompanyPortalSession.PersonaKey);
        ViewBag.PortalPersona = persona;
        ViewBag.StockRefillAlerts = await _db.StockRefillAlerts.AsNoTracking()
            .Include(a => a.Product)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(30)
            .ToListAsync();
        ViewBag.RefillAlertCount = await _db.StockRefillAlerts.AsNoTracking().CountAsync();
        if (persona == CompanyPortalSession.PersonaSales)
        {
            ViewBag.ProductsForRefill = await _db.Products.AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        if (persona == CompanyPortalSession.PersonaAdmin)
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            ViewBag.PendingOrders = await _db.SchoolOrders.AsNoTracking()
                .Include(o => o.SchoolUser)
                .Include(o => o.Lines).ThenInclude(l => l.Product)
                .Where(o => o.Status == SchoolOrderStatus.Pending)
                .OrderBy(o => o.CreatedAtUtc)
                .ToListAsync();
            ViewBag.DecidedOrders = await _db.SchoolOrders.AsNoTracking()
                .Include(o => o.SchoolUser)
                .Include(o => o.Lines).ThenInclude(l => l.Product)
                .Where(o => o.Status != SchoolOrderStatus.Pending)
                .OrderByDescending(o => o.DecidedAtUtc)
                .Take(12)
                .ToListAsync();
            ViewBag.StatPending = await _db.SchoolOrders.AsNoTracking().CountAsync(o => o.Status == SchoolOrderStatus.Pending);
            ViewBag.StatAccepted = await _db.SchoolOrders.AsNoTracking().CountAsync(o => o.Status == SchoolOrderStatus.Accepted);
            ViewBag.StatRejectedMonth = await _db.SchoolOrders.AsNoTracking().CountAsync(o =>
                o.Status == SchoolOrderStatus.Rejected && o.DecidedAtUtc >= monthStart);
            var oldestPendingUtc = await _db.SchoolOrders.AsNoTracking()
                .Where(o => o.Status == SchoolOrderStatus.Pending)
                .OrderBy(o => o.CreatedAtUtc)
                .Select(o => (DateTime?)o.CreatedAtUtc)
                .FirstOrDefaultAsync();
            ViewBag.OldestPendingLabel = oldestPendingUtc is { } t
                ? $"{(DateTime.UtcNow - t).TotalHours:0} h waiting"
                : "—";
        }

        ViewData["BodyClass"] = "app-shell app-shell--company";
        return View("Dashboard");
    }

    [Authorize(Roles = AuthConstants.CompanyStaffRoles)]
    [HttpPost("orders/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOrder(OrderDecisionViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!await PortalSessionMatchesUserAsync(user))
        {
            HttpContext.Session.Remove(CompanyPortalSession.PersonaKey);
            return RedirectToAction(nameof(Login));
        }

        if (HttpContext.Session.GetString(CompanyPortalSession.PersonaKey) != CompanyPortalSession.PersonaAdmin)
        {
            TempData["CompanyError"] = "Only an administrator can approve orders.";
            return RedirectToAction(nameof(Dashboard));
        }

        if (!ModelState.IsValid || model.OrderId < 1)
        {
            TempData["CompanyError"] = "Invalid order.";
            return RedirectToAction(nameof(Dashboard));
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        var order = await _db.SchoolOrders
            .FirstOrDefaultAsync(o => o.Id == model.OrderId);
        if (order is null || order.Status != SchoolOrderStatus.Pending)
        {
            await tx.RollbackAsync();
            TempData["CompanyError"] = "Order not found or already decided.";
            return RedirectToAction(nameof(Dashboard));
        }

        order.Status = SchoolOrderStatus.Accepted;
        order.ApprovedAtUtc = DateTime.UtcNow;
        order.DecisionNote = null;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["CompanySuccess"] = $"Order #{order.Id} accepted for fulfilment.";
        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = AuthConstants.CompanyStaffRoles)]
    [HttpPost("orders/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOrder(OrderDecisionViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!await PortalSessionMatchesUserAsync(user))
        {
            HttpContext.Session.Remove(CompanyPortalSession.PersonaKey);
            return RedirectToAction(nameof(Login));
        }

        if (HttpContext.Session.GetString(CompanyPortalSession.PersonaKey) != CompanyPortalSession.PersonaAdmin)
        {
            TempData["CompanyError"] = "Only an administrator can reject orders.";
            return RedirectToAction(nameof(Dashboard));
        }

        if (!ModelState.IsValid || model.OrderId < 1)
        {
            TempData["CompanyError"] = "Invalid order.";
            return RedirectToAction(nameof(Dashboard));
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        var order = await _db.SchoolOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == model.OrderId);
        if (order is null || order.Status != SchoolOrderStatus.Pending)
        {
            await tx.RollbackAsync();
            TempData["CompanyError"] = "Order not found or already decided.";
            return RedirectToAction(nameof(Dashboard));
        }

        var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        foreach (var line in order.Lines)
        {
            if (products.TryGetValue(line.ProductId, out var product))
                product.StockQuantity += line.Quantity;
        }

        foreach (var pid in productIds)
        {
            if (products.TryGetValue(pid, out var p) && p.StockQuantity > p.ReorderLevel)
                await _db.StockRefillAlerts.Where(a => a.ProductId == pid).ExecuteDeleteAsync();
        }

        order.Status = SchoolOrderStatus.Rejected;
        order.ApprovedAtUtc = DateTime.UtcNow;
        order.DecisionNote = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["CompanySuccess"] = $"Order #{order.Id} rejected; stock has been restored.";
        return RedirectToAction(nameof(Dashboard));
    }

    /// <summary>Sales only: add units to on-hand stock after a delivery or restock.</summary>
    [Authorize(Roles = AuthConstants.CompanyStaffRoles)]
    [HttpPost("stock/refill")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefillStock(RefillStockViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!await PortalSessionMatchesUserAsync(user))
        {
            HttpContext.Session.Remove(CompanyPortalSession.PersonaKey);
            return RedirectToAction(nameof(Login));
        }

        if (HttpContext.Session.GetString(CompanyPortalSession.PersonaKey) != CompanyPortalSession.PersonaSales)
        {
            TempData["CompanyError"] =
                "Only Sales can record stock refills. Administrators receive alerts for visibility — they do not change inventory.";
            return RedirectToAction(nameof(Dashboard));
        }

        if (!ModelState.IsValid)
        {
            TempData["CompanyError"] = "Enter a valid product and quantity to add.";
            return RedirectToAction(nameof(Dashboard));
        }

        var product = await _db.Products.FindAsync(model.ProductId);
        if (product is null)
        {
            TempData["CompanyError"] = "Product not found.";
            return RedirectToAction(nameof(Dashboard));
        }

        product.StockQuantity += model.QuantityAdded;
        await _db.SaveChangesAsync();

        TempData["CompanySuccess"] =
            $"Added {model.QuantityAdded} units to \"{product.Name}\" ({product.Sku}). On-hand is now {product.StockQuantity}.";
        return RedirectToAction(nameof(Dashboard));
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<IActionResult> Login(string? returnUrl = null, string? persona = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (await PortalSessionMatchesUserAsync(user))
                return RedirectToAction(nameof(Dashboard));
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["AuthTheme"] = "company";
        var model = new CompanyLoginViewModel();
        if (persona == CompanyPortalSession.PersonaAdmin || persona == CompanyPortalSession.PersonaSales)
            model.Persona = persona;
        return View(model);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CompanyLoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["AuthTheme"] = "company";

        var domain = $"@{AuthConstants.CompanyEmailDomain}";
        if (!model.Email.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.Email),
                $"Kweena Ka Bobedi staff must use an email ending with {domain}.");
        }

        var persona = model.Persona?.Trim() ?? string.Empty;
        if (persona != CompanyPortalSession.PersonaAdmin && persona != CompanyPortalSession.PersonaSales)
        {
            ModelState.AddModelError(nameof(model.Persona), "Select Administrator or Sales.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        var isStaff = user is not null && (
            await _userManager.IsInRoleAsync(user, AuthConstants.CompanyAdminRole)
            || await _userManager.IsInRoleAsync(user, AuthConstants.CompanySalesRole)
            || await _userManager.IsInRoleAsync(user, AuthConstants.CompanyRole));

        if (!isStaff)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (persona == CompanyPortalSession.PersonaAdmin
            && !await _userManager.IsInRoleAsync(user!, AuthConstants.CompanyAdminRole))
        {
            ModelState.AddModelError(nameof(model.Persona),
                "This account is not enabled for administrator sign-in. Choose Sales or use an administrator account.");
            return View(model);
        }

        if (persona == CompanyPortalSession.PersonaSales
            && !await _userManager.IsInRoleAsync(user!, AuthConstants.CompanySalesRole))
        {
            ModelState.AddModelError(nameof(model.Persona),
                "This account is not enabled for sales sign-in. Choose Administrator or use a sales account.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user!.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        HttpContext.Session.SetString(CompanyPortalSession.PersonaKey, persona);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(Dashboard));
    }

    /// <summary>Legacy URL: redirects to login.</summary>
    [Authorize(Roles = AuthConstants.CompanyStaffRoles)]
    [HttpGet("access")]
    public IActionResult Access()
    {
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = AuthConstants.CompanyStaffRoles)]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove(CompanyPortalSession.PersonaKey);
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.StaffPortal), "Home");
    }

    private async Task<bool> PortalSessionMatchesUserAsync(ApplicationUser? user)
    {
        if (user is null)
            return false;

        var persona = HttpContext.Session.GetString(CompanyPortalSession.PersonaKey);
        if (persona == CompanyPortalSession.PersonaAdmin)
            return await _userManager.IsInRoleAsync(user, AuthConstants.CompanyAdminRole);
        if (persona == CompanyPortalSession.PersonaSales)
            return await _userManager.IsInRoleAsync(user, AuthConstants.CompanySalesRole);
        return false;
    }
}
