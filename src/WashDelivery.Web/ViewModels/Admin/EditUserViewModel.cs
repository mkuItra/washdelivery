using System.ComponentModel.DataAnnotations;
using WashDelivery.Domain.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WashDelivery.Web.ViewModels.Admin;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Imię jest wymagane")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numer telefonu jest wymagany")]
    [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rola jest wymagana")]
    public string Role { get; set; } = string.Empty;

    public Dictionary<string, string> AvailableRoles { get; } = new()
    {
        { Roles.Admin, "Administrator" },
        { Roles.Customer, "Klient" },
        { Roles.Courier, "Kurier" },
        { Roles.LaundryWorker, "Pracownik pralni" },
        { Roles.LaundryManager, "Kierownik pralni" }
    };

    public string? LaundryId { get; set; }
    public List<SelectListItem> AvailableLaundries { get; set; } = new();

    public bool RequiresLaundrySelection(string role)
    {
        return role == Roles.LaundryWorker || role == Roles.LaundryManager;
    }
} 