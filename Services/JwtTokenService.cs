using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoStockIQ.Data;
using AutoStockIQ.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AutoStockIQ.Services;

public class JwtTokenService
{
    private readonly JwtOptions _opt;
    private readonly UserManager<ApplicationUser> _users;

    public JwtTokenService(IOptions<JwtOptions> options, UserManager<ApplicationUser> users)
    {
        _opt = options.Value;
        _users = users;
    }

    public async Task<(string Token, DateTimeOffset ExpiresUtc)?> CreateTokenAsync(
        ApplicationUser user,
        string? companyPersona,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.Key) || _opt.Key.Length < 32)
            return null;

        var roles = await _users.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));
        if (!string.IsNullOrEmpty(companyPersona))
            claims.Add(new Claim(ApiClaimTypes.CompanyPersona, companyPersona));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_opt.ExpiresMinutes);
        var token = new JwtSecurityToken(
            _opt.Issuer,
            _opt.Audience,
            claims,
            expires: expires,
            signingCredentials: creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, new DateTimeOffset(expires, TimeSpan.Zero));
    }
}
