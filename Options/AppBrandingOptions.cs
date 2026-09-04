namespace AutoStockIQ.Options;

/// <summary>Shared branding for web and mobile clients. Override in appsettings.json to match your app theme.</summary>
public class AppBrandingOptions
{
    public const string SectionName = "AppBranding";

    public AppColorTokens Colors { get; set; } = new();

    public AppMobileLinks Mobile { get; set; } = new();
}

public class AppColorTokens
{
    /// <summary>Warm dark (maps to Android <c>text_primary</c> / stone-900).</summary>
    public string Navy { get; set; } = "#1C1917";

    /// <summary>Primary brand orange (Android <c>brand_primary</c> light).</summary>
    public string NavyMid { get; set; } = "#EA580C";

    public string NavyBright { get; set; } = "#FB923C";
    public string NavyDeep { get; set; } = "#9A3412";
    public string NavyGlow { get; set; } = "#F97316";
    public string NavyHover { get; set; } = "#C2410C";
    public string NavyAuthEnd { get; set; } = "#7C2D12";

    /// <summary>School / success green (Android <c>success</c>).</summary>
    public string Teal { get; set; } = "#15803D";

    public string TealLight { get; set; } = "#22C55E";
    public string TealDark { get; set; } = "#166534";
    public string TealAbyss { get; set; } = "#14532D";

    /// <summary>Warning accent (Android <c>warning</c>).</summary>
    public string Accent { get; set; } = "#CA8A04";

    public string AccentSoft { get; set; } = "rgba(202, 138, 4, 0.2)";

    public string Sage { get; set; } = "#FFF7ED";
    public string Paper { get; set; } = "#FFFBF7";
    public string Ink { get; set; } = "#1C1917";
    public string Muted { get; set; } = "#57534E";

    public string Border { get; set; } = "rgba(28, 25, 23, 0.1)";

    public string Surface0 { get; set; } = "#FFFFFF";
    public string Surface1 { get; set; } = "#FFF8F0";
    public string Surface2 { get; set; } = "#FFFBF7";
    public string Surface3 { get; set; } = "#FFEDD5";

    public string AuthCompanyPageBg { get; set; } = "#0C0A09";
    public string AuthSchoolPageBg { get; set; } = "#092F26";

    public string AuthAsideCompanyText { get; set; } = "#FFEDD5";

    /// <summary>Comma-separated RGB (primary orange) for shadows.</summary>
    public string RgbNavyMid { get; set; } = "234, 88, 12";

    /// <summary>Comma-separated RGB (success green) for school-tinted shadows.</summary>
    public string RgbTeal { get; set; } = "21, 128, 61";
}

public class AppMobileLinks
{
    /// <summary>iOS App Store product URL (https://apps.apple.com/...).</summary>
    public string IosAppStoreUrl { get; set; } = "";

    /// <summary>Google Play store URL.</summary>
    public string AndroidPlayStoreUrl { get; set; } = "";

    /// <summary>Optional universal / app link base (e.g. https://app.autostockiq.co.za) for deep links.</summary>
    public string UniversalLinkBase { get; set; } = "";

    public string Tagline { get; set; } =
        "Use AutoStockIQ in the browser or in the mobile app — the same accounts and workflows.";
}
