using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.ValueObjects;

namespace WashDelivery.Application.Interfaces;

public interface IDraftOrderService
{
    Task<DraftOrder> GetOrCreateDraftOrderAsync(string customerId);
    Task UpdateAddressDetailsAsync(
        string draftOrderId,
        OrderAddress pickupAddress,
        OrderAddress deliveryAddress,
        DateTime? pickupTime,
        bool leaveAtDoor,
        string? courierInstructions);
    Task UpdateServicesAsync(string draftOrderId, List<DraftOrderItemDto> items);
    Task<Order> SubmitDraftOrderAsync(string draftOrderId);
} 