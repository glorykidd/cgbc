namespace cgbc.Web.Models;

public static class AdminRoles
{
    /// <summary>
    /// Required to manage other admin accounts (create, edit, delete, reset password).
    /// A compromised or disgruntled non-super admin can't take over every other account.
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";
}
