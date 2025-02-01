using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Web.ViewModels;

public class CustomerAddressesViewModel
{
    public IEnumerable<AddressDto> Addresses { get; set; } = Array.Empty<AddressDto>();
} 