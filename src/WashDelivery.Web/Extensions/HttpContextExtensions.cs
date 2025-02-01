using Microsoft.AspNetCore.Antiforgery;

namespace WashDelivery.Web.Extensions 
{
    public static class HttpContextExtensions
    {
        public static string? GetUserAgent(this HttpContext? httpContext)
             => httpContext?.Request?.Headers["User-Agent"];

        public static string? GetRemoteIpAddress(this HttpContext? httpContext)
            => httpContext?.Connection?.RemoteIpAddress?.ToString();

        public static (string RequestToken, string CookieToken) GetAntiXsrfTokens(this HttpContext context)
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            var tokens = antiforgery.GetAndStoreTokens(context);
            return (tokens.RequestToken, tokens.CookieToken);
        }
    }
}
