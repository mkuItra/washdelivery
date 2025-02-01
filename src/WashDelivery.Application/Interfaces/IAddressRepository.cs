using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface IAddressRepository : IRepository<CustomerDeliveryAddress>
{
    Task<List<CustomerDeliveryAddress>> GetUserAddressesAsync(string userId);
    Task<CustomerDeliveryAddress?> GetDefaultForCustomerAsync(string customerId);
} 