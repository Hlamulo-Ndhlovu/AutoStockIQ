using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AutoStockIQ.Options;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoStockIQ.Controllers.Api;

[ApiController]
[Route("api/app")]
[EnableRateLimiting("api-tight")]
public class AppApiController : ControllerBase
{
    [HttpGet("info")]
    public ActionResult<object> Info([FromServices] IOptions<AppBrandingOptions> branding)
    {
        var b = branding.Value;
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new
        {
            appName = "AutoStockIQ",
            web = new { baseUrl },
            branding = new { colors = b.Colors },
            mobile = new
            {
                b.Mobile.IosAppStoreUrl,
                b.Mobile.AndroidPlayStoreUrl,
                b.Mobile.UniversalLinkBase,
                b.Mobile.Tagline,
            },
            clients = new { android = new { applicationId = "com.example.autostockiq" } },
        });
    }
}
