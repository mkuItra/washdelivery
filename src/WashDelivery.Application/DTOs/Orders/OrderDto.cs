using System;
using System.Collections.Generic;
using System.Linq;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Enums;
using WashDelivery.Domain.ValueObjects;
using WashDelivery.Application.DTOs.Common;

namespace WashDelivery.Application.DTOs.Orders;

public class OrderDto
{
    public string Id { get; set; }
    public string CustomerId { get; set; }
    public string? LaundryId { get; set; }
    public string? CourierId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime PickupTime { get; set; }
    public AddressDto PickupAddress { get; set; }
    public AddressDto DeliveryAddress { get; set; }
    public bool LeaveAtDoor { get; set; }
    public string? CourierInstructions { get; set; }
    public decimal? FinalPrice { get; set; }
    public List<OrderItemDetailsDto> Items { get; set; }
    public List<OrderStatusHistoryDto> StatusHistory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal DeliveryFee { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public static OrderDto FromOrder(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            LaundryId = order.LaundryId,
            CourierId = order.CourierId,
            Status = order.Status,
            PickupTime = order.PickupTime,
            DeliveredAt = order.DeliveredAt,
            PickupAddress = new AddressDto
            {
                Street = order.PickupAddress.Street,
                BuildingNumber = order.PickupAddress.BuildingNumber,
                ApartmentNumber = order.PickupAddress.ApartmentNumber,
                City = order.PickupAddress.City,
                PostalCode = order.PickupAddress.PostalCode
            },
            DeliveryAddress = new AddressDto
            {
                Street = order.DeliveryAddress.Street,
                BuildingNumber = order.DeliveryAddress.BuildingNumber,
                ApartmentNumber = order.DeliveryAddress.ApartmentNumber,
                City = order.DeliveryAddress.City,
                PostalCode = order.DeliveryAddress.PostalCode
            },
            LeaveAtDoor = order.LeaveAtDoor,
            CourierInstructions = order.CourierInstructions,
            FinalPrice = order.FinalPrice,
            DeliveryFee = order.DeliveryFee,
            Items = order.Items.Select(i => new OrderItemDetailsDto
            {
                Id = i.Id,
                Name = i.Name,
                Price = i.Price,
                Weight = i.Weight
            }).ToList(),
            StatusHistory = order.StatusHistory.Select(h => new OrderStatusHistoryDto
            {
                Status = h.Status,
                Timestamp = h.ChangedAt,
                Comment = h.Note
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.DeliveredAt ?? order.CreatedAt
        };
    }
} 