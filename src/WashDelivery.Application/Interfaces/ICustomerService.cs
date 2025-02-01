using WashDelivery.Application.DTOs.Users;
using WashDelivery.Application.DTOs.Common;
using WashDelivery.Application.DTOs.Orders;

namespace WashDelivery.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> RegisterCustomerAsync(CustomerDto customerDto);
    Task<CustomerDto> UpdateCustomerAsync(CustomerDto customerDto);
    Task<IEnumerable<OrderDto>> GetCustomerOrderHistoryAsync(int customerId);
    Task<AddressDto> AddCustomerAddressAsync(int customerId, AddressDto addressDto);
} 