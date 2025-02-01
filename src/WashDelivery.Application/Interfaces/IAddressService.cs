using WashDelivery.Application.DTOs.Common;
namespace WashDelivery.Application.Interfaces;

public interface IAddressService
{
    Task<AddressDto> ValidateAddressAsync(AddressDto address);
    Task<decimal> CalculateDistanceAsync(AddressDto from, AddressDto to);
    Task<IEnumerable<AddressDto>> GetCustomerAddressesAsync(int customerId);
    Task<AddressDto> SetDefaultAddressAsync(int customerId, int addressId);
} 