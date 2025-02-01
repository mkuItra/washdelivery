using Microsoft.AspNetCore.Mvc;

namespace WashDelivery.Web.Controllers
{
    // [ServiceFilter(typeof(AuditLogActionFilter))]
    public class NotFoundController : Controller
    {
        public IActionResult Index() => View();
    }
}