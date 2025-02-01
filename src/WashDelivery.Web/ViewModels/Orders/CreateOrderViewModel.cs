using System.ComponentModel.DataAnnotations;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Web.ViewModels.Orders;

public class CreateOrderViewModel
{
    [Required(ErrorMessage = "Wybierz datę odbioru")]
    [Display(Name = "Data odbioru")]
    public DateTime PickupTime { get; set; }

    [Required(ErrorMessage = "Wybierz adres odbioru")]
    [Display(Name = "Adres odbioru")]
    public string PickupAddressId { get; set; }

    [Required(ErrorMessage = "Wybierz adres dostawy")]
    [Display(Name = "Adres dostawy")]
    public string DeliveryAddressId { get; set; }

    public List<OrderItemViewModel> Items { get; set; } = new();
    public List<AddressDto> AvailableAddresses { get; set; } = new();
}

public class OrderItemViewModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od 0")]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Ilość musi być większa od 0")]
    public int Quantity { get; set; }
} 