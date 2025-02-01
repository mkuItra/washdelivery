using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using WashDelivery.Web.Extensions;

namespace WashDelivery.Web.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = Roles.LaundryManager)]
public class OrderApiController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderApiController> _logger;

    public OrderApiController(IOrderService orderService, ILogger<OrderApiController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost("{orderId}/accept")]
    public async Task<IActionResult> AcceptOrder(string orderId)
    {
        try
        {
            var laundryId = User.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                return NotFound("No laundry assigned to this user");
            }

            var order = await _orderService.AcceptOrderAsync(orderId, laundryId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while accepting the order");
        }
    }

    [HttpPost("{orderId}/decline")]
    public async Task<IActionResult> DeclineOrder(string orderId)
    {
        try
        {
            var laundryId = User.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                return NotFound("No laundry assigned to this user");
            }

            var order = await _orderService.DeclineOrderAsync(orderId, laundryId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error declining order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while declining the order");
        }
    }

    [HttpPost("{orderId}/start-processing")]
    public async Task<IActionResult> StartProcessing(string orderId)
    {
        try
        {
            var laundryId = User.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                return NotFound("No laundry assigned to this user");
            }

            var order = await _orderService.StartProcessingAsync(orderId, laundryId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting processing for order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while starting order processing");
        }
    }

    [HttpPost("{orderId}/mark-ready")]
    public async Task<IActionResult> MarkAsReady(string orderId)
    {
        try
        {
            var laundryId = User.FindFirst("LaundryId")?.Value;
            if (string.IsNullOrEmpty(laundryId))
            {
                return NotFound("No laundry assigned to this user");
            }

            var order = await _orderService.MarkAsReadyAsync(orderId, laundryId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {OrderId} as ready", orderId);
            return StatusCode(500, "An error occurred while marking the order as ready");
        }
    }
} 