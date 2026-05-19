using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Domain.AdminUsers;

public class AdminRole : ValueObject
{
    public static readonly AdminRole Admin = new("Admin");

    public string Value { get; } = string.Empty;

    private AdminRole() { }

    private AdminRole(string value)
    {
        Value = value;
    }

    public static AdminRole FromString(string value)
    {
        if (string.Equals(value, Admin.Value, StringComparison.Ordinal))
        {
            return Admin;
        }

        throw new ArgumentOutOfRangeException(
            nameof(value),
            value,
            "Unknown admin role. Only 'Admin' is defined.");
    }

    public override string ToString() => Value;
}
