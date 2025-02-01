using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WashDelivery.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using WashDelivery.Domain.Entities;
using WashDelivery.Web.ViewModels.Auth;
using WashDelivery.Application.Interfaces;
using WashDelivery.Web.ViewModels;
using System.Security.Claims;
using WashDelivery.Web.Extensions;
using WashDelivery.Application.DTOs.Users;
using WashDelivery.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;
using WashDelivery.Infrastructure.Data;
using WashDelivery.Infrastructure.Services;
using WashDelivery.Domain.Enums;
using WashDelivery.Web.ViewModels.Orders;
using WashDelivery.Application.DTOs.Orders;
using Microsoft.Extensions.Logging;

namespace WashDelivery.Web.Controllers;

[Authorize]
public class PanelController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IUserService _userService;
    private readonly IGeocodingService _geocodingService;
    private readonly AppDbContext _dbContext;
    private readonly IOrderService _orderService;
    private readonly ILaundryService _laundryService;
    private readonly ICourierService _courierService;
    private readonly ILogger<PanelController> _logger;

    public PanelController(
        UserManager<User> userManager,
        IUserService userService,
        IGeocodingService geocodingService,
        AppDbContext dbContext,
        IOrderService orderService,
        ILaundryService laundryService,
        ICourierService courierService,
        ILogger<PanelController> logger)
    {
        _userManager = userManager;
        _userService = userService;
        _geocodingService = geocodingService;
        _dbContext = dbContext;
        _orderService = orderService;
        _laundryService = laundryService;
        _courierService = courierService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains(Roles.Admin))
        {
            try
            {
                var activeLaundries = await _dbContext.Laundries
                    .CountAsync(l => l.IsActive);

                var activeCouriers = await _dbContext.Users
                    .OfType<Courier>()
                    .CountAsync(c => c.IsActive && c.IsAvailable);

                var todayOrders = await _dbContext.Orders
                    .CountAsync(o => o.CreatedAt.Date == DateTime.Today);

                var activeUsers = await _dbContext.Users
                    .CountAsync(u => u.IsActive);

                ViewBag.ActiveLaundries = activeLaundries;
                ViewBag.ActiveCouriers = activeCouriers;
                ViewBag.TodayOrders = todayOrders;
                ViewBag.ActiveUsers = activeUsers;
            }
            catch
            {
                ViewBag.ActiveLaundries = 0;
                ViewBag.ActiveCouriers = 0;
                ViewBag.TodayOrders = 0;
                ViewBag.ActiveUsers = 0;
            }

            return View("AdminPanel");
        }
        else if (roles.Contains(Roles.Courier))
        {
            return RedirectToAction(nameof(CourierPanel));
        }
        else if (roles.Contains(Roles.LaundryManager))
        {
            try
            {
                if (!string.IsNullOrEmpty(user.LaundryId))
                {
                    var laundry = await _dbContext.Laundries
                        .FirstOrDefaultAsync(l => l.Id == user.LaundryId);

                    if (laundry != null)
                    {
                        ViewBag.HasLaundry = true;
                        ViewBag.LaundryIsActive = laundry.IsActive;
                        ViewBag.LaundryName = laundry.Name;
                        ViewBag.LaundryAddress = laundry.Address.ToString();
                        ViewBag.LaundryEmail = laundry.ContactEmail;
                        ViewBag.LaundryPhone = laundry.ContactPhone;

                        var today = DateTime.Today;
                        ViewBag.PendingOrders = await _dbContext.Orders
                            .CountAsync(o => o.LaundryId == user.LaundryId && 
                                           o.Status == OrderStatus.PendingLaundryAssignment);

                        ViewBag.InProgressOrders = await _dbContext.Orders
                            .CountAsync(o => o.LaundryId == user.LaundryId && 
                                           o.Status == OrderStatus.InLaundry);

                        ViewBag.TodayDeliveries = await _dbContext.Orders
                            .CountAsync(o => o.LaundryId == user.LaundryId && 
                                           o.CreatedAt.Date == today &&
                                           (o.Status == OrderStatus.ReadyForDelivery ||
                                            o.Status == OrderStatus.OutForDelivery ||
                                            o.Status == OrderStatus.Delivered));
                    }
                    else
                    {
                        ViewBag.HasLaundry = false;
                    }
                }
                else
                {
                    ViewBag.HasLaundry = false;
                }
            }
            catch
            {
                ViewBag.HasLaundry = false;
            }

            return View("LaundryManagerPanel");
        }
        else if (roles.Contains(Roles.LaundryWorker))
        {
            return View("LaundryWorkerPanel");
        }
        else if (roles.Contains(Roles.Customer))
        {
            return View("CustomerPanel");
        }

        return NotFound();
    }

    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetAddress(string id)
    {
        var user = await _userManager.GetUserAsync(User) as Customer;
        if (user == null)
            return NotFound();

        var address = user.Addresses.FirstOrDefault(a => a.Id == id);
        if (address == null)
            return NotFound();

        return Ok(new AddressDto
        {
            Id = address.Id,
            Name = address.Name,
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            ApartmentNumber = address.ApartmentNumber,
            City = address.City,
            PostalCode = address.PostalCode,
            AdditionalInstructions = address.AdditionalInstructions,
            IsDefault = address.IsDefault
        });
    }

    [HttpPost("AddAddress")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto dto)
    {
        try
        {
            var customer = await _userManager.GetUserAsync(User) as Customer;
            if (customer == null)
                return NotFound();

            var coordinates = await _geocodingService.GetCoordinatesAsync(
                $"{dto.Street} {dto.BuildingNumber}, {dto.PostalCode} {dto.City}");

            var address = new CustomerDeliveryAddress(
                customerId: customer.Id,
                name: dto.Name,
                street: dto.Street,
                buildingNumber: dto.BuildingNumber,
                apartmentNumber: dto.ApartmentNumber,
                city: dto.City,
                postalCode: dto.PostalCode,
                latitude: Convert.ToDecimal(coordinates?.Latitude ?? 0),
                longitude: Convert.ToDecimal(coordinates?.Longitude ?? 0),
                additionalInstructions: dto.AdditionalInstructions,
                isDefault: dto.IsDefault
            );

            customer.AddAddress(address);
            await _userManager.UpdateAsync(customer);

            return Ok();
        }
        catch
        {
            return BadRequest("Error adding address");
        }
    }

    [HttpPost("UpdateAddress")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressDto dto)
    {
        var user = await _userManager.GetUserAsync(User) as Customer;
        if (user == null)
            return NotFound();

        var address = user.Addresses.FirstOrDefault(a => a.Id == dto.Id);
        if (address == null)
            return NotFound();

        if (dto.IsDefault)
        {
            var defaultAddresses = await _dbContext.CustomerDeliveryAddresses
                .Where(a => a.CustomerId == user.Id && a.IsDefault && a.Id != dto.Id)
                .ToListAsync();

            foreach (var defaultAddress in defaultAddresses)
            {
                defaultAddress.SetAsNotDefault();
                _dbContext.CustomerDeliveryAddresses.Update(defaultAddress);
            }
        }

        var coordinates = await _geocodingService.GetCoordinatesAsync(
            $"{dto.Street} {dto.BuildingNumber}, {dto.PostalCode} {dto.City}");

        var updatedAddress = new CustomerDeliveryAddress(
            customerId: user.Id,
            name: dto.Name,
            street: dto.Street,
            buildingNumber: dto.BuildingNumber,
            apartmentNumber: dto.ApartmentNumber,
            city: dto.City,
            postalCode: dto.PostalCode,
            latitude: Convert.ToDecimal(coordinates?.Latitude ?? (double)address.Latitude),
            longitude: Convert.ToDecimal(coordinates?.Longitude ?? (double)address.Longitude),
            additionalInstructions: dto.AdditionalInstructions,
            isDefault: dto.IsDefault
        );

        _dbContext.CustomerDeliveryAddresses.Remove(address);
        await _dbContext.CustomerDeliveryAddresses.AddAsync(updatedAddress);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("DeleteAddress/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> DeleteAddress(string id)
    {
        var user = await _userManager.GetUserAsync(User) as Customer;
        if (user == null)
            return NotFound();

        var address = user.Addresses.FirstOrDefault(a => a.Id == id);
        if (address == null)
            return NotFound();

        if (address.IsDefault)
            return BadRequest("Cannot delete default address");

        user.Addresses.Remove(address);
        await _userManager.UpdateAsync(user);

        return Ok();
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var userSettings = new UserSettingsViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        var addresses = await _dbContext.CustomerDeliveryAddresses
            .Where(a => a.CustomerId == user.Id)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                Name = a.Name,
                Street = a.Street,
                BuildingNumber = a.BuildingNumber,
                ApartmentNumber = a.ApartmentNumber,
                City = a.City,
                PostalCode = a.PostalCode,
                AdditionalInstructions = a.AdditionalInstructions,
                IsDefault = a.IsDefault
            })
            .ToListAsync();

        ViewBag.Addresses = addresses;

        return View(userSettings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(UserSettingsViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        return RedirectToAction(nameof(Settings));
    }

    [Authorize(Roles = Roles.LaundryManager)]
    public async Task<IActionResult> LaundryOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrEmpty(user.LaundryId))
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = await _orderService.GetLaundryOrdersAsync(user.LaundryId);
        
        var dbLaundry = await _dbContext.Laundries
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == user.LaundryId);
            
        if (dbLaundry == null)
        {
            return NotFound("Laundry not found in database");
        }

        // Debug logging
        _logger.LogInformation("Total orders for laundry {LaundryId}: {Count}", user.LaundryId, orders.Count);
        foreach (var order in orders)
        {
            _logger.LogInformation("Order {OrderId} has status {Status}", order.Id, order.Status);
        }

        var completedOrders = orders
            .Where(o => o.Status == OrderStatus.ReadyForDelivery ||
                       o.Status == OrderStatus.OutForDelivery ||
                       o.Status == OrderStatus.Delivered)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        // Debug logging for completed orders
        _logger.LogInformation("Completed orders count: {Count}", completedOrders.Count);
        foreach (var order in completedOrders)
        {
            _logger.LogInformation("Completed order {OrderId} has status {Status}", order.Id, order.Status);
        }

        var viewModel = new LaundryOrderListViewModel
        {
            IsActive = dbLaundry.IsActive,
            PendingOrders = orders
                .Where(o => o.Status == OrderStatus.PendingLaundryAssignment)
                .OrderByDescending(o => o.CreatedAt)
                .ToList(),
            InTransitOrders = orders
                .Where(o => o.Status == OrderStatus.AcceptedByLaundry || 
                           o.Status == OrderStatus.AwaitingPickup || 
                           o.Status == OrderStatus.PickupInProgress ||
                           o.Status == OrderStatus.PickedUp)
                .OrderByDescending(o => o.CreatedAt)
                .ToList(),
            InProgressOrders = orders
                .Where(o => o.Status == OrderStatus.InLaundry)
                .OrderByDescending(o => o.CreatedAt)
                .ToList(),
            CompletedOrders = completedOrders
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LaundryOrders(string handler, string id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrEmpty(user.LaundryId))
        {
            return NotFound("No laundry assigned to this user");
        }

        try
        {
            switch (handler)
            {
                case "Accept":
                    await _orderService.AcceptOrderAsync(id, user.LaundryId);
                    break;
                case "Decline":
                    await _orderService.DeclineOrderAsync(id, user.LaundryId);
                    break;
            }
            
            return RedirectToAction(nameof(LaundryOrders));
        }
        catch
        {
            return RedirectToAction(nameof(LaundryOrders));
        }
    }

    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> CourierPanel()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var courier = await _courierService.GetByIdAsync(userId);
        if (courier == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var availableOrders = new List<OrderDto>();
        var rejectedOrderIds = await _courierService.GetRejectedOrdersAsync(userId);

        if (courier.IsAvailable)
        {
            var unassignedOrders = await _orderService.GetOrdersByStatusesAsync(new[] { 
                OrderStatus.AwaitingPickup, 
                OrderStatus.ReadyForDelivery 
            });

            // Filter out rejected orders and orders with assigned couriers
            availableOrders.AddRange(
                unassignedOrders.Where(o => 
                    string.IsNullOrEmpty(o.CourierId) && 
                    !rejectedOrderIds.Contains(o.Id))
            );
        }

        ViewBag.AvailableOrders = availableOrders;
        ViewBag.IsAvailable = courier.IsAvailable;

        var assignedOrders = await _orderService.GetCourierOrdersAsync(userId);
        var completedOrders = await _orderService.GetOrdersByStatusAsync(OrderStatus.Delivered);

        var today = DateTime.UtcNow.Date;
        var todayDeliveries = completedOrders
            .Where(o => o.CourierId == userId && o.UpdatedAt.Date == today)
            .Count();
        var totalCompletedDeliveries = completedOrders
            .Where(o => o.CourierId == userId)
            .Count();
        var todayEarnings = completedOrders
            .Where(o => o.CourierId == userId && o.UpdatedAt.Date == today)
            .Sum(o => o.DeliveryFee);
        
        var averageDeliveryTime = TimeSpan.Zero;
        if (completedOrders.Any())
        {
            var courierCompletedOrders = completedOrders.Where(o => o.CourierId == userId);
            if (courierCompletedOrders.Any())
            {
                var totalDeliveryTime = courierCompletedOrders
                    .Select(o => o.UpdatedAt - o.CreatedAt)
                    .Aggregate(TimeSpan.Zero, (acc, time) => acc + time);
                averageDeliveryTime = TimeSpan.FromTicks(totalDeliveryTime.Ticks / courierCompletedOrders.Count());
            }
        }

        ViewBag.TodayDeliveries = todayDeliveries;
        ViewBag.CompletedDeliveries = totalCompletedDeliveries;
        ViewBag.TodayEarnings = todayEarnings;
        ViewBag.AverageDeliveryTime = averageDeliveryTime;
        
        ViewBag.AssignedOrders = assignedOrders
            .Where(o => o.CourierId == userId && !rejectedOrderIds.Contains(o.Id))
            .ToList();
        ViewBag.CompletedOrders = completedOrders
            .Where(o => o.CourierId == userId)
            .OrderByDescending(o => o.UpdatedAt)
            .Take(5)
            .ToList();

        return View();
    }

    [HttpPost]
    [Authorize(Roles = Roles.Courier)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Nie znaleziono ID użytkownika";
                return RedirectToAction(nameof(CourierPanel));
            }

            var courier = await _dbContext.Users.OfType<Courier>()
                .FirstOrDefaultAsync(c => c.Id == userId);
            
            if (courier == null)
            {
                TempData["Error"] = "Nie znaleziono kuriera";
                return RedirectToAction(nameof(CourierPanel));
            }

            courier.IsAvailable = !courier.IsAvailable;
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = courier.IsAvailable 
                ? "Jesteś teraz dostępny do przyjmowania zleceń" 
                : "Jesteś teraz niedostępny do przyjmowania zleceń";

            return RedirectToAction(nameof(CourierPanel));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling courier availability");
            TempData["Error"] = "Wystąpił błąd podczas zmiany statusu dostępności";
            return RedirectToAction(nameof(CourierPanel));
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.Courier)]
    public async Task<IActionResult> GetAvailableOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return NotFound();
        }

        var courier = await _courierService.GetByIdAsync(userId);
        if (courier == null || !courier.IsAvailable)
        {
            return PartialView("_AvailableOrders", Enumerable.Empty<OrderDto>());
        }

        var rejectedOrderIds = await _courierService.GetRejectedOrdersAsync(userId);
        var unassignedOrders = await _orderService.GetOrdersByStatusesAsync(new[] { 
            OrderStatus.AwaitingPickup, 
            OrderStatus.ReadyForDelivery 
        });

        var availableOrders = unassignedOrders.Where(o => 
            string.IsNullOrEmpty(o.CourierId) && 
            !rejectedOrderIds.Contains(o.Id));

        return PartialView("_AvailableOrders", availableOrders);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Courier)]
    public async Task<IActionResult> GetActiveOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return NotFound();
        }

        var assignedOrders = await _orderService.GetCourierOrdersAsync(userId);
        var rejectedOrderIds = await _courierService.GetRejectedOrdersAsync(userId);
        var filteredOrders = assignedOrders.Where(o => !rejectedOrderIds.Contains(o.Id));
        return PartialView("_ActiveOrders", filteredOrders);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Courier)]
    public async Task<IActionResult> CompletedOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        // Get completed orders from both Orders and CourierCompletedOrders tables
        var completedOrders = await _dbContext.CourierCompletedOrders
            .Include(co => co.Order)
                .ThenInclude(o => o.PickupAddress)
            .Include(co => co.Order)
                .ThenInclude(o => o.DeliveryAddress)
            .Include(co => co.Order)
                .ThenInclude(o => o.Items)
            .Include(co => co.Order)
                .ThenInclude(o => o.StatusHistory)
            .Where(co => co.CourierId == userId)
            .OrderByDescending(co => co.CompletedAt)
            .Select(co => co.Order)
            .ToListAsync();

        var totalEarnings = completedOrders.Sum(o => o.DeliveryFee);
        var totalDeliveries = completedOrders.Count;
        
        var averageDeliveryTime = TimeSpan.Zero;
        if (completedOrders.Any())
        {
            var ordersWithDeliveryTime = completedOrders
                .Where(o => o.DeliveredAt.HasValue)
                .ToList();

            if (ordersWithDeliveryTime.Any())
            {
                var totalDeliveryTime = ordersWithDeliveryTime
                    .Select(o => o.DeliveredAt!.Value - o.CreatedAt)
                    .Aggregate(TimeSpan.Zero, (acc, time) => acc + time);
                averageDeliveryTime = TimeSpan.FromTicks(totalDeliveryTime.Ticks / ordersWithDeliveryTime.Count);
            }
        }

        // Convert to DTOs using the FromOrder method
        var orderDtos = completedOrders.Select(OrderDto.FromOrder).ToList();

        var viewModel = new CourierCompletedOrdersViewModel
        {
            CompletedOrders = orderDtos,
            TotalEarnings = totalEarnings,
            TotalDeliveries = totalDeliveries,
            AverageDeliveryTime = averageDeliveryTime
        };

        return View(viewModel);
    }
} 