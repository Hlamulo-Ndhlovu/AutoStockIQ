using AutoStockIQ.Data;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class SupplierController : Controller
{
    private readonly FirestoreService _firestoreService;
    private readonly AuditLogService _auditLogService;

    public SupplierController(FirestoreService firestoreService, AuditLogService auditLogService)
    {
        _firestoreService = firestoreService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var suppliers = await _firestoreService.GetAllSuppliersAsync();
        return View(suppliers);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        supplier.Id = await _firestoreService.CreateSupplierAsync(supplier);
        
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.Identity?.Name ?? "";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        
        await _auditLogService.LogAsync(
            userId, userName, userEmail,
            "SupplierCreated", "Supplier", supplier.Id,
            $"Supplier {supplier.Name} created"
        );

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var supplier = await _firestoreService.GetSupplierAsync(id);
        if (supplier == null)
        {
            return NotFound();
        }
        return View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Supplier supplier)
    {
        if (id != supplier.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        await _firestoreService.UpdateSupplierAsync(supplier);
        
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.Identity?.Name ?? "";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        
        await _auditLogService.LogAsync(
            userId, userName, userEmail,
            "SupplierUpdated", "Supplier", supplier.Id,
            $"Supplier {supplier.Name} updated"
        );

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        await _firestoreService.DeactivateSupplierAsync(id);
        
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.Identity?.Name ?? "";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        
        await _auditLogService.LogAsync(
            userId, userName, userEmail,
            "SupplierDeactivated", "Supplier", id,
            $"Supplier {id} deactivated"
        );

        return RedirectToAction(nameof(Index));
    }
}
