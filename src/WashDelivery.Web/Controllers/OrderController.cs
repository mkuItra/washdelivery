using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using System.Security.Claims;
using WashDelivery.Domain.Enums;
using Microsoft.Extensions.Logging;
using WashDelivery.Web.ViewModels.Orders;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.DTOs.Common;
using System.Text.Json;

namespace WashDelivery.Web.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICourierService _courierService;
    private readonly ICustomerAddressRepository _addressRepository;
    private readonly ILogger<OrderController> _logger;
    private readonly ILaundryService _laundryService;

    public OrderController(
        IOrderService orderService, 
        ICourierService courierService,
        ICustomerAddressRepository addressRepository,
        ILogger<OrderController> logger,
        ILaundryService laundryService)
    {
        _orderService = orderService;
        _courierService = courierService;
        _addressRepository = addressRepository;
        _logger = logger;
        _laundryService = laundryService;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> List()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetCustomerOrdersAsync(userId);

            var viewModel = new CustomerOrderListViewModel
            {
                Orders = orders.OrderByDescending(o => o.CreatedAt).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer orders list");
            return RedirectToAction("CustomerPanel", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Courier)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(string orderId)
    {
        _logger.LogInformation("Attempting to accept order {OrderId}", orderId);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            TempData["Error"] = "Nie znaleziono ID użytkownika";
            return RedirectToAction("CourierPanel", "Panel");
        }

        try
        {
            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                TempData["Error"] = "Nie znaleziono zamówienia";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Store original status before any changes
            var originalStatus = order.Status;

            if (originalStatus != OrderStatus.AwaitingPickup && originalStatus != OrderStatus.ReadyForDelivery)
            {
                _logger.LogWarning("Order {OrderId} is in invalid state {Status}", orderId, originalStatus);
                TempData["Error"] = "Zamówienie nie jest dostępne do przyjęcia";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Check if the order is already assigned to this courier
            if (order.CourierId == userId && 
                (order.Status == OrderStatus.PickupInProgress || order.Status == OrderStatus.OutForDelivery))
            {
                _logger.LogInformation("Order {OrderId} is already assigned to courier {UserId}", orderId, userId);
                TempData["Success"] = "Zamówienie jest już przypisane do Ciebie";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Assign courier to the order - this will also update the status appropriately
            order = await _orderService.AssignCourierAsync(orderId, userId);

            _logger.LogInformation("Successfully accepted order {OrderId} by courier {UserId}, status changed from {OldStatus} to {NewStatus}", 
                orderId, userId, originalStatus, order.Status);
            TempData["Success"] = "Pomyślnie przyjęto zamówienie";
            return RedirectToAction("CourierPanel", "Panel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting order {OrderId}: {Message}", orderId, ex.Message);
            TempData["Error"] = "Nie można przyjąć zamówienia";
            return RedirectToAction("CourierPanel", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Courier)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string orderId)
    {
        try
        {
            _logger.LogInformation("Attempting to reject order {OrderId}", orderId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                TempData["Error"] = "Nie znaleziono ID użytkownika";
                return RedirectToAction("CourierPanel", "Panel");
            }

            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                TempData["Error"] = "Nie znaleziono zamówienia";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Add the order to courier's rejected orders list
            var result = await _courierService.AddRejectedOrderAsync(userId, orderId);
            if (!result)
            {
                _logger.LogWarning("Failed to add order {OrderId} to rejected list for courier {UserId}", orderId, userId);
                TempData["Error"] = "Nie udało się odrzucić zamówienia";
                return RedirectToAction("CourierPanel", "Panel");
            }

            _logger.LogInformation("Successfully rejected order {OrderId} by courier {UserId}", orderId, userId);
            TempData["Success"] = "Pomyślnie odrzucono zamówienie";
            return RedirectToAction("CourierPanel", "Panel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting order {OrderId}", orderId);
            TempData["Error"] = "Wystąpił błąd podczas odrzucania zamówienia";
            return RedirectToAction("CourierPanel", "Panel");
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Check if user has access to this order
            if (User.IsInRole(Roles.Customer) && order.CustomerId != userId)
            {
                return Forbid();
            }
            if (User.IsInRole(Roles.Courier) && order.CourierId != userId)
            {
                return Forbid();
            }

            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details for order {OrderId}", id);
            return RedirectToAction("List");
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Create()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var addresses = await _addressRepository.GetByCustomerIdAsync(userId);
            var addressDtos = addresses.Select(a => new AddressDto
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
            }).ToList();

            var viewModel = new CreateOrderViewModel
            {
                PickupTime = DateTime.Now.AddHours(1),
                AvailableAddresses = addressDtos
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing order creation form");
            TempData["Error"] = "Wystąpił błąd podczas przygotowywania formularza zamówienia";
            return RedirectToAction("CustomerPanel", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Reload available addresses
                model.AvailableAddresses = new List<AddressDto>();
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var createOrderDto = new CreateOrderDto
            {
                PickupAddressId = model.PickupAddressId,
                DeliveryAddressId = model.DeliveryAddressId,
                ScheduledDateTime = model.PickupTime,
                Items = model.Items.Select(i => new CreateOrderItemDto
                {
                    Name = i.Name,
                    Price = i.Price,
                    Weight = i.Quantity // Using Quantity as Weight for now
                }).ToList()
            };

            var order = await _orderService.CreateOrderAsync(userId, createOrderDto);
            
            TempData["Success"] = "Zamówienie zostało utworzone pomyślnie";
            return RedirectToAction("Details", new { id = order.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            TempData["Error"] = "Wystąpił błąd podczas tworzenia zamówienia";
            return RedirectToAction("CustomerPanel", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAddressDetails([FromBody] SaveAddressDetailsViewModel model)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Validate addresses belong to the user
            var addresses = await _addressRepository.GetByCustomerIdAsync(userId);
            var pickupAddress = addresses.FirstOrDefault(a => a.Id == model.PickupAddressId);
            var deliveryAddress = addresses.FirstOrDefault(a => a.Id == model.DeliveryAddressId);

            if (pickupAddress == null || deliveryAddress == null)
            {
                return BadRequest("Invalid address selection");
            }

            // Store the address details in TempData for the next step
            var addressDetails = new OrderAddressDetailsDto
            {
                PickupAddressId = model.PickupAddressId,
                DeliveryAddressId = model.DeliveryAddressId,
                PickupTime = model.PickupTimeOption == "scheduled" ? DateTime.Parse(model.PickupTime!) : DateTime.Now.AddHours(1),
                LeaveAtDoor = model.LeaveAtDoor,
                CourierInstructions = model.CourierInstructions
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

            TempData["AddressDetails"] = JsonSerializer.Serialize(addressDetails, jsonOptions);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving address details");
            return StatusCode(500, "An error occurred while saving address details");
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> LaundryDetails()
    {
        try
        {
            // Get address details from TempData
            var addressDetailsJson = TempData["AddressDetails"] as string;
            if (string.IsNullOrEmpty(addressDetailsJson))
            {
                return RedirectToAction(nameof(Create));
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

            var addressDetails = JsonSerializer.Deserialize<OrderAddressDetailsDto>(addressDetailsJson, jsonOptions);
            if (addressDetails == null)
            {
                return RedirectToAction(nameof(Create));
            }

            // Store back in TempData for the next request
            TempData["AddressDetails"] = addressDetailsJson;

            // Get available laundries
            var laundries = await _laundryService.GetAvailableLaundriesAsync();
            var viewModel = new LaundryDetailsViewModel
            {
                AvailableLaundries = laundries.ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing laundry details form");
            TempData["Error"] = "Wystąpił błąd podczas przygotowywania formularza wyboru usług";
            return RedirectToAction("CustomerPanel", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [ValidateAntiForgeryToken]
    public IActionResult SaveServices([FromBody] SaveServicesViewModel model)
    {
        try
        {
            _logger.LogInformation("Received SaveServices request");

            if (model == null)
            {
                _logger.LogWarning("Model is null");
                return Json(new { success = false, message = "Invalid request data" });
            }

            if (model.Items == null)
            {
                _logger.LogWarning("Items is null");
                return Json(new { success = false, message = "No services data provided" });
            }

            if (!model.Items.Any())
            {
                _logger.LogWarning("Items is empty");
                return Json(new { success = false, message = "No services selected" });
            }

            _logger.LogInformation("Received {Count} services", model.Items.Count);

            // Get address details from TempData
            var addressDetailsJson = TempData["AddressDetails"] as string;
            if (string.IsNullOrEmpty(addressDetailsJson))
            {
                _logger.LogWarning("Address details not found in TempData");
                return Json(new { success = false, message = "Address details not found" });
            }

            // Store back in TempData for the next request
            TempData["AddressDetails"] = addressDetailsJson;

            // Store services in TempData
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

            try
            {
                var servicesJson = JsonSerializer.Serialize(model.Items, jsonOptions);
                _logger.LogInformation("Successfully serialized services");
                TempData["Services"] = servicesJson;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serializing services");
                return Json(new { success = false, message = "Error processing service data" });
            }

            _logger.LogInformation("Successfully saved services");
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving services");
            return Json(new { success = false, message = "An error occurred while saving services" });
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Summary()
    {
        try
        {
            // Get address details from TempData
            var addressDetailsJson = TempData["AddressDetails"] as string;
            if (string.IsNullOrEmpty(addressDetailsJson))
            {
                return RedirectToAction(nameof(Create));
            }

            // Get services from TempData
            var servicesJson = TempData["Services"] as string;
            if (string.IsNullOrEmpty(servicesJson))
            {
                return RedirectToAction(nameof(LaundryDetails));
            }

            // Store back in TempData for the next request
            TempData["AddressDetails"] = addressDetailsJson;
            TempData["Services"] = servicesJson;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

            var addressDetails = JsonSerializer.Deserialize<OrderAddressDetailsDto>(addressDetailsJson, jsonOptions);
            var services = JsonSerializer.Deserialize<List<ServiceItemViewModel>>(servicesJson, jsonOptions);

            if (addressDetails == null || services == null)
            {
                return RedirectToAction(nameof(Create));
            }

            // Get addresses
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var addresses = await _addressRepository.GetByCustomerIdAsync(userId);
            var pickupAddress = addresses.FirstOrDefault(a => a.Id == addressDetails.PickupAddressId);
            var deliveryAddress = addresses.FirstOrDefault(a => a.Id == addressDetails.DeliveryAddressId);

            if (pickupAddress == null || deliveryAddress == null)
            {
                return RedirectToAction(nameof(Create));
            }

            var viewModel = new OrderSummaryViewModel
            {
                PickupAddress = new AddressDto
                {
                    Id = pickupAddress.Id,
                    Name = pickupAddress.Name,
                    Street = pickupAddress.Street,
                    BuildingNumber = pickupAddress.BuildingNumber,
                    ApartmentNumber = pickupAddress.ApartmentNumber,
                    City = pickupAddress.City,
                    PostalCode = pickupAddress.PostalCode,
                    AdditionalInstructions = pickupAddress.AdditionalInstructions
                },
                DeliveryAddress = new AddressDto
                {
                    Id = deliveryAddress.Id,
                    Name = deliveryAddress.Name,
                    Street = deliveryAddress.Street,
                    BuildingNumber = deliveryAddress.BuildingNumber,
                    ApartmentNumber = deliveryAddress.ApartmentNumber,
                    City = deliveryAddress.City,
                    PostalCode = deliveryAddress.PostalCode,
                    AdditionalInstructions = deliveryAddress.AdditionalInstructions
                },
                PickupTime = addressDetails.PickupTime,
                LeaveAtDoor = addressDetails.LeaveAtDoor,
                CourierInstructions = addressDetails.CourierInstructions,
                Services = services
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing order summary");
            TempData["Error"] = "Wystąpił błąd podczas przygotowywania podsumowania zamówienia";
            return RedirectToAction("CustomerPanel", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit()
    {
        try
        {
            _logger.LogInformation("Attempting to submit order");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            // Get data from TempData
            var addressDetailsJson = TempData["AddressDetails"] as string;
            var servicesJson = TempData["Services"] as string;

            if (string.IsNullOrEmpty(addressDetailsJson) || string.IsNullOrEmpty(servicesJson))
            {
                return Json(new { success = false, message = "Order details not found" });
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

            var addressDetails = JsonSerializer.Deserialize<OrderAddressDetailsDto>(addressDetailsJson, jsonOptions);
            var services = JsonSerializer.Deserialize<List<ServiceItemViewModel>>(servicesJson, jsonOptions);

            if (addressDetails == null || services == null)
            {
                return Json(new { success = false, message = "Invalid order data" });
            }

            var createOrderDto = new CreateOrderDto
            {
                PickupAddressId = addressDetails.PickupAddressId,
                DeliveryAddressId = addressDetails.DeliveryAddressId,
                ScheduledDateTime = addressDetails.PickupTime,
                LeaveAtDoor = addressDetails.LeaveAtDoor,
                CourierInstructions = addressDetails.CourierInstructions,
                Items = services.Select(s => new CreateOrderItemDto
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Price = s.Price,
                    Weight = s.Quantity
                }).ToList()
            };

            var order = await _orderService.CreateOrderAsync(userId, createOrderDto);

            // Clear TempData after successful order creation
            TempData.Clear();

            return Json(new { success = true, orderId = order.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return Json(new { success = false, message = "An error occurred while creating the order" });
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Courier)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPickup(string orderId)
    {
        _logger.LogInformation("Attempting to confirm pickup for order {OrderId}", orderId);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            TempData["Error"] = "Nie znaleziono ID użytkownika";
            return RedirectToAction("CourierPanel", "Panel");
        }

        try
        {
            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                TempData["Error"] = "Nie znaleziono zamówienia";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Verify that this courier is assigned to this order
            if (order.CourierId != userId)
            {
                _logger.LogWarning("Courier {UserId} attempted to confirm pickup for order {OrderId} assigned to different courier", 
                    userId, orderId);
                TempData["Error"] = "Nie masz uprawnień do tego zamówienia";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Verify order is in correct state
            if (order.Status != OrderStatus.PickupInProgress)
            {
                _logger.LogWarning("Order {OrderId} is in invalid state {Status} for pickup confirmation", orderId, order.Status);
                TempData["Error"] = "Zamówienie nie jest w trakcie odbioru";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Update order status to PickedUp
            order = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.PickedUp);

            _logger.LogInformation("Successfully confirmed pickup for order {OrderId} by courier {UserId}", orderId, userId);
            TempData["Success"] = "Pomyślnie potwierdzono odbiór zamówienia";
            return RedirectToAction("CourierPanel", "Panel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming pickup for order {OrderId}: {Message}", orderId, ex.Message);
            TempData["Error"] = "Nie można potwierdzić odbioru zamówienia";
            return RedirectToAction("CourierPanel", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.LaundryManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmArrival(string orderId)
    {
        _logger.LogInformation("Attempting to confirm order arrival at laundry for order {OrderId}", orderId);
        _logger.LogInformation("User: {User}, Roles: {Roles}", User.Identity?.Name, string.Join(", ", User.Claims.Where(c => c.Type == "role").Select(c => c.Value)));

        try
        {
            if (string.IsNullOrEmpty(orderId))
            {
                _logger.LogWarning("OrderId is null or empty");
                TempData["Error"] = "Nieprawidłowy identyfikator zamówienia";
                return RedirectToAction("LaundryOrders", "Panel");
            }

            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                TempData["Error"] = "Nie znaleziono zamówienia";
                return RedirectToAction("LaundryOrders", "Panel");
            }

            _logger.LogInformation("Found order {OrderId} with status {Status}", orderId, order.Status);

            // Verify order is in correct state
            if (order.Status != OrderStatus.PickedUp)
            {
                _logger.LogWarning("Order {OrderId} is in invalid state {Status} for arrival confirmation", orderId, order.Status);
                TempData["Error"] = "Zamówienie nie jest w trakcie transportu do pralni";
                return RedirectToAction("LaundryOrders", "Panel");
            }

            // Update order status to InLaundry
            _logger.LogInformation("Updating order {OrderId} status to InLaundry", orderId);
            order = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.InLaundry);
            _logger.LogInformation("Successfully updated order {OrderId} status to {Status}", orderId, order.Status);

            _logger.LogInformation("Successfully confirmed arrival at laundry for order {OrderId}", orderId);
            TempData["Success"] = "Pomyślnie potwierdzono odbiór zamówienia w pralni";
            return RedirectToAction("LaundryOrders", "Panel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming arrival at laundry for order {OrderId}: {Message}", orderId, ex.Message);
            TempData["Error"] = "Nie można potwierdzić odbioru zamówienia w pralni";
            return RedirectToAction("LaundryOrders", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.LaundryManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsReady(string orderId)
    {
        _logger.LogInformation("Attempting to mark order {OrderId} as ready for pickup", orderId);

        try
        {
            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                TempData["Error"] = "Nie znaleziono zamówienia";
                return RedirectToAction("LaundryOrders", "Panel");
            }

            // Verify order is in correct state
            if (order.Status != OrderStatus.InLaundry)
            {
                _logger.LogWarning("Order {OrderId} is in invalid state {Status} for marking as ready", orderId, order.Status);
                TempData["Error"] = "Zamówienie nie jest w pralni";
                return RedirectToAction("LaundryOrders", "Panel");
            }

            // Update order status to ReadyForDelivery
            order = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.ReadyForDelivery);

            _logger.LogInformation("Successfully marked order {OrderId} as ready for pickup", orderId);
            TempData["Success"] = "Pomyślnie oznaczono zamówienie jako gotowe do odbioru";
            return RedirectToAction("LaundryOrders", "Panel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {OrderId} as ready: {Message}", orderId, ex.Message);
            TempData["Error"] = "Nie można oznaczyć zamówienia jako gotowe do odbioru";
            return RedirectToAction("LaundryOrders", "Panel");
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Courier)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDelivery(string orderId)
    {
        _logger.LogInformation("Attempting to confirm delivery for order {OrderId}", orderId);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            TempData["Error"] = "Nie znaleziono ID użytkownika";
            return RedirectToAction("CourierPanel", "Panel");
        }

        try
        {
            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                TempData["Error"] = "Nie znaleziono zamówienia";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Verify that this courier is assigned to this order
            if (order.CourierId != userId)
            {
                _logger.LogWarning("Courier {UserId} attempted to confirm delivery for order {OrderId} assigned to different courier", 
                    userId, orderId);
                TempData["Error"] = "Nie masz uprawnień do tego zamówienia";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Verify order is in correct state
            if (order.Status != OrderStatus.OutForDelivery)
            {
                _logger.LogWarning("Order {OrderId} is in invalid state {Status} for delivery confirmation", orderId, order.Status);
                TempData["Error"] = "Zamówienie nie jest w trakcie dostawy";
                return RedirectToAction("CourierPanel", "Panel");
            }

            // Update order status to Delivered and set delivery timestamp
            order = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Delivered);

            _logger.LogInformation("Successfully confirmed delivery for order {OrderId} by courier {UserId}", orderId, userId);
            TempData["Success"] = "Pomyślnie potwierdzono dostarczenie zamówienia";
            return RedirectToAction("CourierPanel", "Panel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming delivery for order {OrderId}: {Message}", orderId, ex.Message);
            TempData["Error"] = "Nie można potwierdzić dostarczenia zamówienia";
            return RedirectToAction("CourierPanel", "Panel");
        }
    }

    [HttpPut("api/orders/{orderId}/status")]
    [Authorize(Roles = Roles.Customer)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Verify that this customer owns this order
            if (order.CustomerId != userId)
            {
                return Forbid();
            }

            // Only allow cancellation of pending orders
            if (dto.NewStatus == OrderStatus.Cancelled)
            {
                if (order.Status != OrderStatus.PendingLaundryAssignment)
                {
                    return BadRequest("Can only cancel orders that are pending laundry assignment");
                }
            }
            else
            {
                return BadRequest("Invalid status update");
            }

            order = await _orderService.UpdateOrderStatusAsync(orderId, dto.NewStatus, dto.Comment);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status: {Message}", orderId, ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    public class AcceptOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
    }

    public class RejectOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
    }
} 