using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WashDelivery.Domain.Entities;
using WashDelivery.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using WashDelivery.Application.DTOs.Auth;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using WashDelivery.Web.ViewModels.Auth;
using System.Security.Claims;
using System.Linq;

namespace WashDelivery.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IAuthService _authService;

    public AccountController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IAuthService authService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Panel");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Nieprawidłowa próba logowania.");
            return View(model);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Konto zostało dezaktywowane. Skontaktuj się z administratorem.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Add or update LaundryId claim if user has a laundry assigned
            if (!string.IsNullOrEmpty(user.LaundryId))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var existingClaim = claims.FirstOrDefault(c => c.Type == "LaundryId");
                
                if (existingClaim != null && existingClaim.Value != user.LaundryId)
                {
                    // Remove old claim if value is different
                    await _userManager.RemoveClaimAsync(user, existingClaim);
                    existingClaim = null;
                }
                
                if (existingClaim == null)
                {
                    // Add new claim
                    var laundryIdClaim = new Claim("LaundryId", user.LaundryId);
                    await _userManager.AddClaimAsync(user, laundryIdClaim);
                }
            }
            else
            {
                // Remove LaundryId claim if user no longer has a laundry
                var claims = await _userManager.GetClaimsAsync(user);
                var existingClaim = claims.FirstOrDefault(c => c.Type == "LaundryId");
                if (existingClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, existingClaim);
                }
            }

            // Add CourierId claim if user is a courier
            if (await _userManager.IsInRoleAsync(user, Roles.Courier))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var existingClaim = claims.FirstOrDefault(c => c.Type == "CourierId");
                
                if (existingClaim == null)
                {
                    // Add new claim
                    var courierIdClaim = new Claim("CourierId", user.Id);
                    await _userManager.AddClaimAsync(user, courierIdClaim);
                }
            }

            // Refresh the sign-in to ensure all claims are included
            await _signInManager.RefreshSignInAsync(user);

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Panel");
            return RedirectToLocal(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Nieprawidłowa próba logowania.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterCustomer()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Panel");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterCustomer(CustomerRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Hasła nie są takie same");
            return View(model);
        }

        var dto = new RegisterCustomerDto
        {
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber
        };

        var result = await _authService.RegisterCustomerAsync(dto);
        if (!result.success)
        {
            foreach (var error in result.errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterCourier()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Panel");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterCourier(CourierRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Hasła nie są takie same");
            return View(model);
        }

        var dto = new RegisterCourierDto
        {
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber
        };

        var result = await _authService.RegisterCourierAsync(dto);
        if (!result.success)
        {
            foreach (var error in result.errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User(
            email: model.Email,
            phoneNumber: model.PhoneNumber,
            firstName: model.FirstName,
            lastName: model.LastName
        );

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        else
            return RedirectToAction(nameof(HomeController.Index), "Home");
    }
} 