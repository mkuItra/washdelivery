using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Constants;
using System.Security.Claims;

namespace WashDelivery.Web.Middleware;

public class LaundryAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LaundryAuthorizationMiddleware> _logger;

    public LaundryAuthorizationMiddleware(RequestDelegate next, ILogger<LaundryAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Validate security stamp
                    var securityStampValid = await signInManager.ValidateSecurityStampAsync(context.User);
                    if (securityStampValid == null)
                    {
                        _logger.LogWarning("Security stamp validation failed for user {UserId}", userId);
                        await HandleUnauthorizedAccess(context);
                        return;
                    }

                    var laundryIdClaim = context.User.FindFirst("LaundryId");
                    if (laundryIdClaim != null)
                    {
                        var userRoles = await userManager.GetRolesAsync(user);
                        var isLaundryRole = userRoles.Any(r => r == Roles.LaundryWorker || r == Roles.LaundryManager);

                        if (user.LaundryId != laundryIdClaim.Value || !isLaundryRole)
                        {
                            _logger.LogWarning(
                                "User {UserId} authorization failed: LaundryMatch={LaundryMatch}, HasLaundryRole={HasLaundryRole}",
                                userId,
                                user.LaundryId == laundryIdClaim.Value,
                                isLaundryRole);

                            // Sign out the user
                            await signInManager.SignOutAsync();

                            if (context.Request.Path.StartsWithSegments("/hubs"))
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            }
                            else
                            {
                                await HandleUnauthorizedAccess(context);
                            }
                            return;
                        }
                    }
                    else if (!string.IsNullOrEmpty(user.LaundryId))
                    {
                        // Add LaundryId claim if missing
                        var userRoles = await userManager.GetRolesAsync(user);
                        var isLaundryRole = userRoles.Any(r => r == Roles.LaundryWorker || r == Roles.LaundryManager);

                        if (isLaundryRole)
                        {
                            _logger.LogInformation(
                                "Adding missing LaundryId claim for user {UserId}. LaundryId: {LaundryId}",
                                userId,
                                user.LaundryId);

                            var claim = new Claim("LaundryId", user.LaundryId);
                            await userManager.AddClaimAsync(user, claim);
                            await signInManager.RefreshSignInAsync(user);
                        }
                    }
                }
            }
        }

        await _next(context);
    }

    private async Task HandleUnauthorizedAccess(HttpContext context)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            context.Request.Headers["Accept"].Any(h => h.Contains("application/json")))
        {
            await context.Response.WriteAsJsonAsync(new { message = "Your session has expired. Please log in again." });
        }
        else
        {
            context.Response.Redirect("/Account/Login");
        }
    }
} 