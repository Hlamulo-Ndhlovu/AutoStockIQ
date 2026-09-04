using System.Diagnostics;
using AutoStockIQ.Data;
using AutoStockIQ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoStockIQ.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
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
                    var persona = HttpContext.Session.GetString(CompanyPortalSession.PersonaKey);
                    if (persona == CompanyPortalSession.PersonaAdmin
                        && await _userManager.IsInRoleAsync(user, AuthConstants.CompanyAdminRole))
                        ViewBag.ShowCompanyDashboard = true;
                    else if (persona == CompanyPortalSession.PersonaSales && await _userManager.IsInRoleAsync(user, AuthConstants.CompanySalesRole))
                        ViewBag.ShowCompanyDashboard = true;
                }
            }

            return View();
        }

        /// <summary>School-only entry: registration and sign-in. No company login on this path.</summary>
        public IActionResult SchoolPortal()
        {
            return View();
        }

        /// <summary>Business-only entry: registration and sign-in. No company login on this path.</summary>
        public IActionResult BusinessPortal()
        {
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
