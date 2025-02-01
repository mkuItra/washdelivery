using System.Collections.Generic;
using System.Threading.Tasks;
using WashDelivery.Domain.Entities;

namespace WashDelivery.Application.Interfaces;

public interface ICustomerAddressRepository
{
    Task<CustomerDeliveryAddress?> GetByIdAsync(string id);
    Task<List<CustomerDeliveryAddress>> GetByCustomerIdAsync(string customerId);
    Task<CustomerDeliveryAddress> AddAsync(CustomerDeliveryAddress address);
    Task<CustomerDeliveryAddress> UpdateAsync(CustomerDeliveryAddress address);
    Task DeleteAsync(string id);
} 