using System.ComponentModel.DataAnnotations;

namespace WashDelivery.Application.DTOs.Common;

public class CreateAddressDto
{
    [Required(ErrorMessage = "Nazwa adresu jest wymagana")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ulica jest wymagana")]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numer budynku jest wymagany")]
    [StringLength(20)]
    public string BuildingNumber { get; set; } = string.Empty;

    [StringLength(20)]
    public string? ApartmentNumber { get; set; }

    [Required(ErrorMessage = "Miasto jest wymagane")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kod pocztowy jest wymagany")]
    [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Nieprawidłowy format kodu pocztowego (XX-XXX)")]
    public string PostalCode { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AdditionalInstructions { get; set; }

    public bool IsDefault { get; set; }
} 