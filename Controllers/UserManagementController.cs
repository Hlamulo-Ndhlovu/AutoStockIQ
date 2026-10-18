using AutoStockIQ.Data;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AuditLogService _auditLogService;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AuditLogService auditLogService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserManagementViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                SchoolName = user.SchoolName,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                PopiaConsent = user.PopiaConsent,
                CreatedAtUtc = user.CreatedAtUtc
            });
        }

        return View(userViewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new CreateUserViewModel
        {
            AvailableRoles = new List<string> { AuthConstants.AdminRole, AuthConstants.SchoolUserRole, AuthConstants.BusinessUserRole, AuthConstants.StaffRole }
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = new List<string> { AuthConstants.AdminRole, AuthConstants.SchoolUserRole, AuthConstants.BusinessUserRole, AuthConstants.StaffRole };
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            SchoolName = model.SchoolName,
            PopiaConsent = model.PopiaConsent,
            PopiaConsentDateUtc = DateTime.UtcNow,
            PopiaConsentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", string.Join(", ", result.Errors.Select(e => e.Description)));
            model.AvailableRoles = new List<string> { AuthConstants.AdminRole, AuthConstants.SchoolUserRole, AuthConstants.BusinessUserRole, AuthConstants.StaffRole };
            return View(model);
        }

        if (model.SelectedRoles != null && model.SelectedRoles.Any())
        {
            await _userManager.AddToRolesAsync(user, model.SelectedRoles);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.Identity?.Name ?? "";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";

        await _auditLogService.LogAsync(
            userId, userName, userEmail,
            "UserCreated", "User", user.Id,
            $"User {model.Email} created with roles: {string.Join(", ", model.SelectedRoles ?? new List<string>())}"
        );

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var viewModel = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            SchoolName = user.SchoolName,
            SelectedRoles = roles.ToList(),
            AvailableRoles = new List<string> { AuthConstants.AdminRole, AuthConstants.SchoolUserRole, AuthConstants.BusinessUserRole, AuthConstants.StaffRole },
            IsActive = user.IsActive
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = new List<string> { AuthConstants.AdminRole, AuthConstants.SchoolUserRole, AuthConstants.BusinessUserRole, AuthConstants.StaffRole };
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Email = model.Email;
        user.UserName = model.Email;
        user.SchoolName = model.SchoolName;
        user.IsActive = model.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", string.Join(", ", result.Errors.Select(e => e.Description)));
            model.AvailableRoles = new List<string> { AuthConstants.AdminRole, AuthConstants.SchoolUserRole, AuthConstants.BusinessUserRole, AuthConstants.StaffRole };
            return View(model);
        }

        // Update roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

        if (rolesToAdd.Any())
        {
            await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        if (rolesToRemove.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.Identity?.Name ?? "";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";

        await _auditLogService.LogAsync(
            userId, userName, userEmail,
            "UserUpdated", "User", user.Id,
            $"User {model.Email} updated. Roles: {string.Join(", ", model.SelectedRoles)}"
        );

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.Identity?.Name ?? "";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";

        await _auditLogService.LogAsync(
            userId, userName, userEmail,
            "UserDeactivated", "User", user.Id,
            $"User {user.Email} deactivated"
        );

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var newPassword = Guid.NewGuid().ToString().Substring(0, 8) + "!Aa1";

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", string.Join(", ", result.Errors.Select(e => e.Description)));
            return RedirectToAction(nameof(Index));
        }

        TempData["NewPassword"] = $"Password reset for {user.Email}. New password: {newPassword}";
        return RedirectToAction(nameof(Index));
    }
}

public class UserManagementViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public bool PopiaConsent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateUserViewModel
{
    public string Email { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool PopiaConsent { get; set; }
    public List<string>? SelectedRoles { get; set; }
    public List<string> AvailableRoles { get; set; } = new();
}

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public List<string> SelectedRoles { get; set; } = new();
    public List<string> AvailableRoles { get; set; } = new();
    public bool IsActive { get; set; }
}
