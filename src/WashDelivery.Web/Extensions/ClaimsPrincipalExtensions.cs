using System.Security.Claims;

namespace WashDelivery.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null)
            throw new ArgumentNullException(nameof(principal));

        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim?.Value ?? throw new InvalidOperationException("User ID not found in claims");
    }

    public static string GetUserEmail(this ClaimsPrincipal principal)
    {
        if (principal == null)
            throw new ArgumentNullException(nameof(principal));

        var claim = principal.FindFirst(ClaimTypes.Email);
        return claim?.Value ?? throw new InvalidOperationException("Email not found in claims");
    }

    public static string GetUserName(this ClaimsPrincipal principal)
    {
        if (principal == null)
            throw new ArgumentNullException(nameof(principal));

        var claim = principal.FindFirst(ClaimTypes.Name);
        return claim?.Value ?? throw new InvalidOperationException("Username not found in claims");
    }
}
