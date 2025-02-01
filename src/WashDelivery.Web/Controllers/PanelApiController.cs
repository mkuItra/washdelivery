using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WashDelivery.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using WashDelivery.Domain.Entities;
using WashDelivery.Application.DTOs.Users;
using WashDelivery.Application.DTOs.Common;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using WashDelivery.Infrastructure.Data;
using WashDelivery.Web.Extensions;

namespace WashDelivery.Web.Controllers;

[Route("api/panel")]
[ApiController]
public class PanelApiController : ControllerBase
{
    private readonly ILogger<PanelApiController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IGeocodingService _geocodingService;
    private readonly AppDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserService _userService;
    private readonly ILaundryService _laundryService;

    public PanelApiController(
        ILogger<PanelApiController> logger,
        UserManager<User> userManager,
        IGeocodingService geocodingService,
        AppDbContext dbContext,
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        IUserService userService,
        ILaundryService laundryService)
    {
        _logger = logger;
        _userManager = userManager;
        _geocodingService = geocodingService;
        _dbContext = dbContext;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _userService = userService;
        _laundryService = laundryService;
    }

    [HttpGet("addresses/{id}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetAddress(string id)
    {
        var user = await _userManager.GetUserAsync(User) as Customer;
        if (user == null)
            return BadRequest("User is not a customer");

        var address = await _dbContext.CustomerDeliveryAddresses
            .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == user.Id);

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

    [HttpPost("addresses")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto dto)
    {
        try 
        {
            var user = await _userManager.GetUserAsync(User) as Customer;
            if (user == null)
                return BadRequest("User is not a customer");

            var coordinates = await _geocodingService.GetCoordinatesAsync(
                $"{dto.Street} {dto.BuildingNumber}, {dto.PostalCode} {dto.City}");

            var hasExistingAddresses = await _dbContext.CustomerDeliveryAddresses
                .AnyAsync(a => a.CustomerId == user.Id);

            if (dto.IsDefault)
            {
                var defaultAddresses = await _dbContext.CustomerDeliveryAddresses
                    .Where(a => a.CustomerId == user.Id && a.IsDefault)
                    .ToListAsync();

                foreach (var defaultAddress in defaultAddresses)
                {
                    defaultAddress.IsDefault = false;
                    _dbContext.CustomerDeliveryAddresses.Update(defaultAddress);
                }
            }

            var address = new CustomerDeliveryAddress(
                customerId: user.Id,
                name: dto.Name,
                street: dto.Street,
                buildingNumber: dto.BuildingNumber,
                apartmentNumber: dto.ApartmentNumber,
                city: dto.City,
                postalCode: dto.PostalCode,
                latitude: Convert.ToDecimal(coordinates?.Latitude ?? 0),
                longitude: Convert.ToDecimal(coordinates?.Longitude ?? 0),
                additionalInstructions: dto.AdditionalInstructions,
                isDefault: !hasExistingAddresses || dto.IsDefault
            );

            _dbContext.CustomerDeliveryAddresses.Add(address);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error adding address");
        }
    }

    [HttpPut("addresses/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressDto dto)
    {
        var user = await _userManager.GetUserAsync(User) as Customer;
        if (user == null)
            return BadRequest("User is not a customer");

        var address = await _dbContext.CustomerDeliveryAddresses
            .FirstOrDefaultAsync(a => a.Id == dto.Id && a.CustomerId == user.Id);

        if (address == null)
            return NotFound();

        var coordinates = await _geocodingService.GetCoordinatesAsync(
            $"{dto.Street} {dto.BuildingNumber}, {dto.PostalCode} {dto.City}");

        address.Update(
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

        _dbContext.CustomerDeliveryAddresses.Update(address);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("addresses/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> DeleteAddress(string id)
    {
        var user = await _userManager.GetUserAsync(User) as Customer;
        if (user == null)
            return BadRequest("User is not a customer");

        var address = await _dbContext.CustomerDeliveryAddresses
            .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == user.Id);

        if (address == null)
            return NotFound();

        if (address.IsDefault)
            return BadRequest("Cannot delete default address");

        _dbContext.CustomerDeliveryAddresses.Remove(address);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("users/{userId}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await _userRepository.GetByIdAsync<User>(userId);
        if (user == null)
            return NotFound();

        return Ok(new UserDto
        {
            // ... mapping
        });
    }

    [HttpGet("orders/{id}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetOrder(string id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpPost("users/{userId}/laundry/{laundryId}")]
    public async Task<IActionResult> AssignLaundryToUser(string userId, string laundryId)
    {
        var success = await _userService.AssignLaundryAsync(userId, laundryId);
        if (!success)
            return NotFound();

        return Ok();
    }

    [HttpPost("users/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync<User>(userId);
        if (user == null)
            return NotFound();

        // Update user details
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;

        await _userRepository.UpdateAsync(user);
        return Ok();
    }

    [HttpGet("GetUserAddresses")]
    public async Task<IActionResult> GetUserAddresses()
    {
        var addresses = await _userService.GetUserAddressesAsync(User.GetUserId());
        return Ok(addresses);
    }

    [HttpPost("laundry/toggle-status")]
    [Authorize(Roles = Roles.LaundryManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLaundryStatus()
    {
        try
        {
            var laundryId = User.GetUserId();
            var laundry = await _dbContext.Laundries.FindAsync(laundryId);
            
            if (laundry == null)
            {
                _logger.LogWarning("Laundry not found for user {UserId}", laundryId);
                return NotFound("Laundry not found");
            }

            laundry.SetActive(!laundry.IsActive);
            _dbContext.Laundries.Update(laundry);
            await _dbContext.SaveChangesAsync();

            return Ok(new { isActive = laundry.IsActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling laundry status");
            return StatusCode(500, "Error updating laundry status");
        }
    }

    [HttpPost("courier/toggle-availability")]
    [Authorize(Roles = Roles.Courier)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCourierAvailability()
    {
        try
        {
            var courierId = User.GetUserId();
            var courier = await _dbContext.Users.OfType<Courier>()
                .FirstOrDefaultAsync(c => c.Id == courierId);
            
            if (courier == null)
            {
                _logger.LogWarning("Courier not found for user {UserId}", courierId);
                return NotFound("Courier not found");
            }

            courier.IsAvailable = !courier.IsAvailable;
            _dbContext.Users.Update(courier);
            await _dbContext.SaveChangesAsync();

            return Ok(new { isAvailable = courier.IsAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling courier availability");
            return StatusCode(500, "Error updating courier availability");
        }
    }
} 