using System.ComponentModel.DataAnnotations;
using WashDelivery.Domain.Constants;

namespace WashDelivery.Web.ViewModels.Admin;

public class CreateUserViewModel
{
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

    [Required(ErrorMessage = "Hasło jest wymagane")]
    [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane")]
    [Compare("Password", ErrorMessage = "Hasła nie są identyczne")]
    public string ConfirmPassword { get; set; } = string.Empty;

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
} 