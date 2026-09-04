namespace AutoStockIQ.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = "";

    public string Issuer { get; set; } = "AutoStockIQ";

    public string Audience { get; set; } = "AutoStockIQ.Mobile";

    public int ExpiresMinutes { get; set; } = 60 * 24 * 7;
}
