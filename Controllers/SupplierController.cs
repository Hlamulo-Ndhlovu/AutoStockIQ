using AutoStockIQ.Data;
using AutoStockIQ.Models.ViewModels;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = AuthConstants.StaffRoles)]
[Route("suppliers")]
public class SupplierController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly AuditLogService _auditLog;

    public SupplierController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        AuditLogService auditLog)
    {
        _userManager = userManager;
        _db = db;
        _auditLog = auditLog;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        var suppliers = await _db.Suppliers.AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
        return View(suppliers);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        return View();
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        if (!ModelState.IsValid)
            return View(model);

        var supplier = new Supplier
        {
            Id = Guid.NewGuid().ToString(),
            Name = model.Name,
            ContactPerson = model.ContactPerson,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _db.Suppliers.AddAsync(supplier);
        await _db.SaveChangesAsync();

        await _auditLog.LogAsync(user.Id, user.UserName ?? "Unknown", user.Email ?? "unknown", "CREATE", "Supplier", supplier.Id, 
            $"Created supplier: {supplier.Name}");

        TempData["SupplierSuccess"] = $"Supplier '{supplier.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound();

        var model = new SupplierViewModel
        {
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Email = supplier.Email,
            PhoneNumber = supplier.PhoneNumber,
            Address = supplier.Address
        };

        return View(model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, SupplierViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        supplier.Name = model.Name;
        supplier.ContactPerson = model.ContactPerson;
        supplier.Email = model.Email;
        supplier.PhoneNumber = model.PhoneNumber;
        supplier.Address = model.Address;

        await _db.SaveChangesAsync();

        await _auditLog.LogAsync(user.Id, user.UserName ?? "Unknown", user.Email ?? "unknown", "UPDATE", "Supplier", supplier.Id,
            $"Updated supplier: {supplier.Name}");

        TempData["SupplierSuccess"] = $"Supplier '{supplier.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("deactivate/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound();

        supplier.IsActive = false;
        supplier.DeactivatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _auditLog.LogAsync(user.Id, user.UserName ?? "Unknown", user.Email ?? "unknown", "DEACTIVATE", "Supplier", supplier.Id,
            $"Deactivated supplier: {supplier.Name}");

        TempData["SupplierSuccess"] = $"Supplier '{supplier.Name}' deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("activate/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound();

        supplier.IsActive = true;
        supplier.DeactivatedAtUtc = null;

        await _db.SaveChangesAsync();

        await _auditLog.LogAsync(user.Id, user.UserName ?? "Unknown", user.Email ?? "unknown", "ACTIVATE", "Supplier", supplier.Id,
            $"Activated supplier: {supplier.Name}");

        TempData["SupplierSuccess"] = $"Supplier '{supplier.Name}' activated successfully.";
        return RedirectToAction(nameof(Index));
    }
}