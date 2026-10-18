namespace AutoStockIQ.Data;

public class UserSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    /// <summary>User's preferred theme: "light", "dark", or "auto"</summary>
    public string Theme { get; set; } = "auto";

    /// <summary>User's preferred language code (e.g., "en", "af", "zu")</summary>
    public string Language { get; set; } = "en";

    /// <summary>Whether to receive email notifications for order updates</summary>
    public bool EmailNotificationsEnabled { get; set; } = true;

    /// <summary>Whether to receive email notifications for low stock alerts</summary>
    public bool LowStockAlertsEnabled { get; set; } = true;

    /// <summary>Whether to receive email notifications for new orders</summary>
    public bool NewOrderAlertsEnabled { get; set; } = true;

    /// <summary>Items per page in lists (for admins/sales)</summary>
    public int ItemsPerPage { get; set; } = 20;

    /// <summary>Default currency code for display</summary>
    public string CurrencyCode { get; set; } = "ZAR";

    /// <summary>Whether to show compact view on mobile</summary>
    public bool CompactViewEnabled { get; set; } = false;

    /// <summary>Whether to enable two-factor authentication</summary>
    public bool TwoFactorEnabled { get; set; } = false;

    /// <summary>Phone number for 2FA (if enabled)</summary>
    public string? TwoFactorPhoneNumber { get; set; }

    /// <summary>Whether the user has completed onboarding</summary>
    public bool OnboardingCompleted { get; set; } = false;

    /// <summary>When user settings were last updated</summary>
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}
