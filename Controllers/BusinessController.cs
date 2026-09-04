using AutoStockIQ.Data;
using AutoStockIQ.Models.ViewModels;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Route("business")]
public class BusinessController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly OrderPlacementService _orders;

    public BusinessController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        OrderPlacementService orders)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
        _orders = orders;
    }

    [Authorize(Roles = AuthConstants.BusinessUserRole)]
    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();
        ViewBag.BusinessName = user.BusinessName;
        ViewBag.Email = user.Email;
        ViewBag.SubmittedOrders = await _db.SchoolOrders.AsNoTracking()
            .CountAsync(o => o.SchoolUserId == user.Id && o.Status == SchoolOrderStatus.Submitted);
        ViewBag.ApprovedOrders = await _db.SchoolOrders.AsNoTracking()
            .CountAsync(o => o.SchoolUserId == user.Id && o.Status == SchoolOrderStatus.Approved);
        ViewBag.FulfilledOrders = await _db.SchoolOrders.AsNoTracking()
            .CountAsync(o => o.SchoolUserId == user.Id && o.Status == SchoolOrderStatus.Fulfilled);
        ViewBag.CancelledOrders = await _db.SchoolOrders.AsNoTracking()
            .CountAsync(o => o.SchoolUserId == user.Id && o.Status == SchoolOrderStatus.Cancelled);
        ViewData["BodyClass"] = "app-shell app-shell--business";
        return View("Dashboard");
    }

    [Authorize(Roles = AuthConstants.BusinessUserRole)]
    [HttpGet("orders")]
    public async Task<IActionResult> Orders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();
        ViewData["BodyClass"] = "app-shell app-shell--business";
        var orders = await _db.SchoolOrders.AsNoTracking()
            .Where(o => o.SchoolUserId == user.Id)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();
        return View(orders);
    }

    [Authorize(Roles = AuthConstants.BusinessUserRole)]
    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog()
    {
        ViewData["BodyClass"] = "app-shell app-shell--business";
        var products = await _db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        return View(products);
    }

    [Authorize(Roles = AuthConstants.BusinessUserRole)]
    [HttpPost("order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(PlaceSchoolOrderViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        if (!ModelState.IsValid)
        {
            TempData["OrderError"] = "Check the quantity and try again.";
            return RedirectToAction(nameof(Catalog));
        }

        var result = await _orders.PlaceSchoolOrderAsync(user.Id, model.ProductId, model.Quantity);
        if (!result.Success)
        {
            TempData["OrderError"] = result.Error;
            return RedirectToAction(nameof(Catalog));
        }

        TempData["OrderSuccess"] = result.RefillAlertRaised
            ? "Order placed. Kweena Ka Bobedi has been notified (low stock). Sales will record refills when stock arrives."
            : "Order placed successfully.";
        return RedirectToAction(nameof(Catalog));
    }

    [AllowAnonymous]
    [HttpGet("register")]
    public IActionResult Register()
    {
        ViewData["AuthTheme"] = "business";
        return View();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(BusinessRegisterViewModel model)
    {
        ViewData["AuthTheme"] = "business";
        
        // Manual POPIA consent validation
        if (!model.PopiaConsent)
        {
            ModelState.AddModelError(nameof(model.PopiaConsent), "You must agree to the POPIA consent to continue.");
        }
        
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            BusinessName = model.BusinessName,
            EmailConfirmed = true,
            PopiaConsent = model.PopiaConsent,
            PopiaConsentDateUtc = DateTime.UtcNow,
            PopiaConsentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, AuthConstants.BusinessUserRole);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(Dashboard));
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["AuthTheme"] = "business";
        return View();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(BusinessLoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["AuthTheme"] = "business";
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !await _userManager.IsInRoleAsync(user, AuthConstants.BusinessUserRole))
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = AuthConstants.BusinessUserRole)]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.BusinessPortal), "Home");
    }
}