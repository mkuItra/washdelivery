using System.ComponentModel.DataAnnotations;

namespace WashDelivery.Web.ViewModels.Auth;

public class CustomerRegistrationViewModel
{
    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane")]
    [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane")]
    [Compare("Password", ErrorMessage = "Hasła nie są takie same")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Imię jest wymagane")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numer telefonu jest wymagany")]
    [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu")]
    public string PhoneNumber { get; set; } = string.Empty;
} 