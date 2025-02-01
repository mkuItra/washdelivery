using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using WashDelivery.Domain.Enums;
using WashDelivery.Domain.Entities;
using WashDelivery.Web.Extensions;

namespace WashDelivery.Web.Controllers;

[Authorize(Roles = Roles.LaundryManager)]
[Route("[controller]")]
public class LaundryManagerController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ILogger<LaundryManagerController> _logger;
    private readonly ILaundryService _laundryService;
    private readonly UserManager<User> _userManager;

    public LaundryManagerController(
        IOrderService orderService,
        ILogger<LaundryManagerController> logger,
        ILaundryService laundryService,
        UserManager<User> userManager)
    {
        _orderService = orderService;
        _logger = logger;
        _laundryService = laundryService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var laundry = await _laundryService.GetByIdAsync(user.LaundryId);
            if (laundry == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(laundry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LaundryManager Index action");
            return RedirectToAction("Error", "Home");
        }
    }

    private string? GetLaundryId()
    {
        return User.FindFirst("LaundryId")?.Value 
            ?? User.FindFirst("laundryid")?.Value 
            ?? User.FindFirst("laundryId")?.Value;
    }

    [HttpGet("GetStatistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var laundryId = GetLaundryId();
            if (string.IsNullOrEmpty(laundryId))
            {
                var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                _logger.LogWarning("No LaundryId claim found for user {UserId}. Available claims: {Claims}", 
                    User.GetUserId(), 
                    string.Join(", ", allClaims));
                return NotFound("No laundry assigned to this user");
            }

            var orders = await _orderService.GetLaundryOrdersAsync(laundryId);

            var pendingOrders = orders.Count(o => o.Status == OrderStatus.PendingLaundryAssignment);
            var inProgressOrders = orders.Count(o => 
                o.Status == OrderStatus.AcceptedByLaundry || 
                o.Status == OrderStatus.InLaundry);
            var todayDeliveries = orders.Count(o => 
                o.Status == OrderStatus.ReadyForDelivery && 
                o.PickupTime.Date == DateTime.Today);

            return Json(new
            {
                pendingOrders,
                inProgressOrders,
                todayDeliveries
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting laundry statistics");
            return StatusCode(500, "Error getting statistics");
        }
    }

    [HttpPost("Activate")]
    public async Task<IActionResult> Activate()
    {
        try
        {
            _logger.LogInformation("TEST LOG - Activate method started");
            
            var laundryId = GetLaundryId();
            if (string.IsNullOrEmpty(laundryId))
            {
                var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                _logger.LogWarning("No LaundryId claim found for user {UserId}. Available claims: {Claims}", 
                    User.GetUserId(), 
                    string.Join(", ", allClaims));
                return NotFound("No laundry assigned to this user");
            }

            await _laundryService.ActivateAsync(laundryId);
            _logger.LogInformation("Laundry {LaundryId} activated", laundryId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating laundry");
            return StatusCode(500, "Error activating laundry");
        }
    }

    [HttpPost("Deactivate")]
    public async Task<IActionResult> Deactivate()
    {
        try
        {
            var laundryId = GetLaundryId();
            if (string.IsNullOrEmpty(laundryId))
            {
                var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                _logger.LogWarning("No LaundryId claim found for user {UserId}. Available claims: {Claims}", 
                    User.GetUserId(), 
                    string.Join(", ", allClaims));
                return NotFound("No laundry assigned to this user");
            }

            await _laundryService.DeactivateAsync(laundryId);
            _logger.LogInformation("Laundry {LaundryId} deactivated", laundryId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating laundry");
            return StatusCode(500, "Error deactivating laundry");
        }
    }
} 