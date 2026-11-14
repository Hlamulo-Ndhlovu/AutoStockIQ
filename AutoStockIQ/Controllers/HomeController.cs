using System.Diagnostics;
using AutoStockIQ.Data;
using AutoStockIQ.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoStockIQ.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            await SetPortalFlagsAsync();
            return View();
        }

        /// <summary>Staff-only entry: leads to company sign-in. Kept separate from school flows.</summary>
        public async Task<IActionResult> StaffPortal()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is not null)
                {
                    var isStaff = await IsStaffAsync(user);
                    var isSchool = await _userManager.IsInRoleAsync(user, AuthConstants.SchoolUserRole);

                    // School users must not land on the staff portal while signed in.
                    if (isSchool && !isStaff)
                        return RedirectToAction("Dashboard", "School");

                    if (isStaff)
                    {
                        var persona = HttpContext.Session.GetString(CompanyPortalSession.PersonaKey);
                        if (persona == CompanyPortalSession.PersonaAdmin
                            && await _userManager.IsInRoleAsync(user, AuthConstants.CompanyAdminRole))
                            ViewBag.ShowCompanyDashboard = true;
                        else if (persona == CompanyPortalSession.PersonaSales
                                 && await _userManager.IsInRoleAsync(user, AuthConstants.CompanySalesRole))
                            ViewBag.ShowCompanyDashboard = true;
                    }
                }
            }

            return View();
        }

        /// <summary>School-only entry: registration and sign-in. No company login on this path.</summary>
        public async Task<IActionResult> SchoolPortal()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is not null)
                {
                    var isStaff = await IsStaffAsync(user);
                    var isSchool = await _userManager.IsInRoleAsync(user, AuthConstants.SchoolUserRole);

                    // Staff must not land on the school portal while signed in.
                    if (isStaff && !isSchool)
                        return RedirectToAction("Dashboard", "Company");

                    if (isSchool)
                        return RedirectToAction("Dashboard", "School");
                }
            }

            return View();
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove(CompanyPortalSession.PersonaKey);
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task SetPortalFlagsAsync()
        {
            ViewBag.IsGuest = User.Identity?.IsAuthenticated != true;
            if (ViewBag.IsGuest)
                return;

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                ViewBag.IsGuest = true;
                return;
            }

            ViewBag.IsStaffUser = await IsStaffAsync(user);
            ViewBag.IsSchoolUser = await _userManager.IsInRoleAsync(user, AuthConstants.SchoolUserRole);
        }

        private async Task<bool> IsStaffAsync(ApplicationUser user) =>
            await _userManager.IsInRoleAsync(user, AuthConstants.AdminRole)
            || await _userManager.IsInRoleAsync(user, AuthConstants.StaffRole);
    }
}
