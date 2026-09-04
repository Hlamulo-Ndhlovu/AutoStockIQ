using AutoStockIQ.Data;
using AutoStockIQ.Models.Api;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoStockIQ.Controllers.Api;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("api-tight")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwt;

    public AuthApiController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwt)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
    }

    [AllowAnonymous]
    [HttpPost("school/register")]
    public async Task<ActionResult<AuthResponse>> SchoolRegister(
        [FromBody] SchoolRegisterRequest model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            SchoolName = model.SchoolName,
            EmailConfirmed = true,
            PopiaConsent = model.PopiaConsent,
            PopiaConsentDateUtc = DateTime.UtcNow,
            PopiaConsentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, AuthConstants.SchoolRole);

        var token = await _jwt.CreateTokenAsync(user, companyPersona: null, cancellationToken);
        if (token is null)
            return StatusCode(500, new { error = "Token service not configured (Jwt:Key)." });

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new AuthResponse
        {
            Token = token.Value.Token,
            ExpiresAtUtc = token.Value.ExpiresUtc,
            Email = user.Email ?? "",
            SchoolName = user.SchoolName,
            Roles = roles.ToList(),
        });
    }

    [AllowAnonymous]
    [HttpPost("school/login")]
    public async Task<ActionResult<AuthResponse>> SchoolLogin(
        [FromBody] LoginRequest model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !await _userManager.IsInRoleAsync(user, AuthConstants.SchoolRole))
            return Unauthorized(new { error = "Invalid login attempt." });

        var check = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
        if (!check.Succeeded)
            return Unauthorized(new { error = "Invalid login attempt." });

        var token = await _jwt.CreateTokenAsync(user, companyPersona: null, cancellationToken);
        if (token is null)
            return StatusCode(500, new { error = "Token service not configured (Jwt:Key)." });

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new AuthResponse
        {
            Token = token.Value.Token,
            ExpiresAtUtc = token.Value.ExpiresUtc,
            Email = user.Email ?? "",
            SchoolName = user.SchoolName,
            Roles = roles.ToList(),
        });
    }

    [AllowAnonymous]
    [HttpPost("company/login")]
    public async Task<ActionResult<AuthResponse>> CompanyLogin(
        [FromBody] CompanyLoginRequest model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var domain = $"@{AuthConstants.CompanyEmailDomain}";
        if (!model.Email.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = $"Staff must use an email ending with {domain}." });

        var persona = model.Persona.Trim();
        if (persona != CompanyPortalSession.PersonaAdmin && persona != CompanyPortalSession.PersonaSales)
            return BadRequest(new { error = "Persona must be Admin or Sales." });

        var user = await _userManager.FindByEmailAsync(model.Email);
        var isStaff = user is not null && (
            await _userManager.IsInRoleAsync(user, AuthConstants.CompanyAdminRole)
            || await _userManager.IsInRoleAsync(user, AuthConstants.CompanySalesRole)
            || await _userManager.IsInRoleAsync(user, AuthConstants.CompanyRole));

        if (!isStaff)
            return Unauthorized(new { error = "Invalid login attempt." });

        if (persona == CompanyPortalSession.PersonaAdmin
            && !await _userManager.IsInRoleAsync(user!, AuthConstants.CompanyAdminRole))
            return BadRequest(new { error = "This account is not enabled for administrator sign-in." });

        if (persona == CompanyPortalSession.PersonaSales
            && !await _userManager.IsInRoleAsync(user!, AuthConstants.CompanySalesRole))
            return BadRequest(new { error = "This account is not enabled for sales sign-in." });

        var check = await _signInManager.CheckPasswordSignInAsync(user!, model.Password, lockoutOnFailure: true);
        if (!check.Succeeded)
            return Unauthorized(new { error = "Invalid login attempt." });

        var token = await _jwt.CreateTokenAsync(user!, persona, cancellationToken);
        if (token is null)
            return StatusCode(500, new { error = "Token service not configured (Jwt:Key)." });

        var roles = await _userManager.GetRolesAsync(user!);
        return Ok(new AuthResponse
        {
            Token = token.Value.Token,
            ExpiresAtUtc = token.Value.ExpiresUtc,
            Email = user!.Email ?? "",
            CompanyPersona = persona,
            Roles = roles.ToList(),
        });
    }
}
