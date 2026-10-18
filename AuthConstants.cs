namespace AutoStockIQ;

public static class AuthConstants
{
    /// <summary>Admin role - full system access including user management, order approval, and inventory management</summary>
    public const string AdminRole = "Admin";

    /// <summary>School User role - can create purchase orders and view order history</summary>
    public const string SchoolUserRole = "SchoolUser";

    /// <summary>Business User role - can create purchase orders and view order history</summary>
    public const string BusinessUserRole = "BusinessUser";

    /// <summary>Staff role - can manage inventory, process sales, and view dashboard</summary>
    public const string StaffRole = "Staff";

    public const string StaffRoles = AdminRole + "," + StaffRole;

    /// <summary>Staff may only sign in with addresses on this domain (e.g. name@autostockiq.co.za).</summary>
    public const string CompanyEmailDomain = "autostockiq.co.za";

    // Legacy role constants for backward compatibility
    public const string SchoolRole = "SchoolUser";
    public const string CompanyAdminRole = "Admin";
    public const string CompanySalesRole = "Staff";
    public const string CompanyStaffRoles = "Admin,Staff";
    public const string CompanyRole = "Admin";
}
