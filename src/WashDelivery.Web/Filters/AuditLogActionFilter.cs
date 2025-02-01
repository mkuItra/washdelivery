using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace WashDelivery.Web.Filters
{
    public class AuditLogActionFilter : IActionFilter
    {
        private readonly ILogger<AuditLogActionFilter> _logger;

        public AuditLogActionFilter(ILogger<AuditLogActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Removed logging
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Implementation if needed
        }
    }
}
