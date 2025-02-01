using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WashDelivery.Application.DTOs.Users;
using WashDelivery.Application.DTOs.Laundries;
using WashDelivery.Application.DTOs.Common;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using WashDelivery.Web.ViewModels.Admin;
using Microsoft.AspNetCore.RateLimiting;

namespace WashDelivery.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly IUserService _userService;
    private readonly ILaundryService _laundryService;

    public AdminController(
        ILogger<AdminController> logger,
        IUserService userService,
        ILaundryService laundryService)
    {
        _logger = logger;
        _userService = userService;
        _laundryService = laundryService;
    }

    [HttpGet]
    public async Task<IActionResult> Users(string? role = null)
    {
        var users = await _userService.GetUsersAsync(role, User.Identity?.Name);
        var viewModel = new UsersViewModel
        {
            Users = users,
            SelectedRole = role
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        if (User.Identity?.Name == null)
            return BadRequest();

        var currentUser = await _userService.GetUserByIdAsync(id);
        if (currentUser.Email == User.Identity.Name)
        {
            ModelState.AddModelError(string.Empty, "Nie możesz deaktywować swojego konta.");
            return RedirectToAction(nameof(Users));
        }

        await _userService.DeactivateUserAsync(id);
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    public async Task<IActionResult> ActivateUser(string id)
    {
        await _userService.ActivateUserAsync(id);
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _userService.CreateUserAsync(new CreateUserDto
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password,
                Role = model.Role
            });

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas tworzenia użytkownika.");
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userService.GetUserRolesAsync(id);
        var laundries = await _laundryService.GetAllAsync();

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? string.Empty,
            LaundryId = user.LaundryId,
            AvailableLaundries = laundries
                .Where(l => l.IsActive)
                .Select(l => new SelectListItem
                {
                    Value = l.Id,
                    Text = l.Name
                })
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(string id, EditUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _userService.UpdateUserAsync(new UpdateUserDto
            {
                Id = id,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Role = model.Role,
                LaundryId = model.LaundryId
            });

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas aktualizacji użytkownika.");
        }

        var laundries = await _laundryService.GetAllAsync();
        model.AvailableLaundries = laundries
            .Where(l => l.IsActive)
            .Select(l => new SelectListItem
            {
                Value = l.Id,
                Text = l.Name
            })
            .ToList();

        return View(model);
    }

    [EnableRateLimiting("search")]
    [Authorize(Roles = Roles.Admin)]
    [ValidateAntiForgeryToken]
    [HttpGet]
    public async Task<IActionResult> SearchUsers(string term, string roles)
    {
        // Walidacja dozwolonych ról
        var allowedRoles = new[] { Roles.LaundryWorker, Roles.LaundryManager };
        var requestedRoles = roles.Split(',');
        
        // Sprawdź czy wszystkie żądane role są dozwolone
        if (requestedRoles.Any(role => !allowedRoles.Contains(role)))
        {
            return BadRequest("Niedozwolone role");
        }

        var users = await _userService.SearchUsersAsync(term, requestedRoles, excludeAssigned: true);
        return Json(users);
    }
} 