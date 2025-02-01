using Microsoft.AspNetCore.Mvc;
using WashDelivery.Infrastructure.Data;

namespace WashDelivery.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DevController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("db-info")]
        public IActionResult GetDbInfo()
        {
            var info = new
            {
                Laundries = _context.Laundries.Count(),
                Users = _context.Users.Count(),
                Roles = _context.Roles.Count(),
                // ... inne tabele
            };
            return Ok(info);
        }
    }
} 