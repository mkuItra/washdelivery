using System.ComponentModel.DataAnnotations;
using WashDelivery.Application.DTOs.Users;
using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Web.ViewModels;

public class UserSettingsViewModel
{
    [Required(ErrorMessage = "Imię jest wymagane")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu")]
    public string PhoneNumber { get; set; } = string.Empty;
} 