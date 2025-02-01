using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.Enums;
using WashDelivery.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace WashDelivery.Application.Services;

public class OrderService : OrderQueries, IOrderService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerAddressRepository _addressRepository;
    private readonly UserManager<User> _userManager;
    private readonly ICourierNotificationService _courierNotificationService;
    private readonly ILogger<OrderService> _logger;
    private readonly IMapper _mapper;
    private readonly ILaundryNotificationService _laundryNotificationService;
    private readonly ICourierService _courierService;

    public OrderService(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        ICustomerAddressRepository addressRepository,
        UserManager<User> userManager,
        ICourierNotificationService courierNotificationService,
        ILogger<OrderService> logger,
        IMapper mapper,
        ILaundryNotificationService laundryNotificationService,
        ICourierService courierService)
        : base(orderRepository)
    {
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _userManager = userManager;
        _courierNotificationService = courierNotificationService;
        _logger = logger;
        _mapper = mapper;
        _laundryNotificationService = laundryNotificationService;
        _courierService = courierService;
    }

    public async Task<OrderDto> CreateOrderAsync(string customerId, CreateOrderDto dto)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", customerId);

            var pickupAddress = await _addressRepository.GetByIdAsync(dto.PickupAddressId);
            var deliveryAddress = await _addressRepository.GetByIdAsync(dto.DeliveryAddressId);

            if (pickupAddress == null || deliveryAddress == null)
            {
                throw new InvalidOperationException("Invalid addresses");
            }

            var order = new Order(
                customerId: customerId,
                pickupAddress: new OrderAddress(
                    pickupAddress.Street,
                    pickupAddress.BuildingNumber,
                    pickupAddress.ApartmentNumber,
                    pickupAddress.City,
                    pickupAddress.PostalCode,
                    pickupAddress.Latitude,
                    pickupAddress.Longitude,
                    pickupAddress.AdditionalInstructions
                ),
                deliveryAddress: new OrderAddress(
                    deliveryAddress.Street,
                    deliveryAddress.BuildingNumber,
                    deliveryAddress.ApartmentNumber,
                    deliveryAddress.City,
                    deliveryAddress.PostalCode,
                    deliveryAddress.Latitude,
                    deliveryAddress.Longitude,
                    deliveryAddress.AdditionalInstructions
                ),
                pickupTime: dto.ScheduledDateTime,
                leaveAtDoor: dto.LeaveAtDoor,
                courierInstructions: dto.CourierInstructions
            );

            foreach (var item in dto.Items)
            {
                order.AddItem(
                    name: item.Name,
                    price: item.Price,
                    weight: item.Weight ?? 0,
                    serviceId: item.ServiceId
                );
            }

            _logger.LogInformation("Adding order {OrderId} to repository. Status: {Status}, Items: {ItemCount}", 
                order.Id, order.Status, order.Items.Count);
            await _orderRepository.AddAsync(order);

            var orderDto = OrderDto.FromOrder(order);

            // Immediately try to notify available laundry
            var notified = await _laundryNotificationService.TryNotifyAvailableLaundryAsync(orderDto);
            if (!notified)
            {
                // If no active laundry was found, update order status
                await UpdateOrderStatusAsync(order.Id, OrderStatus.LaundryRejected, "No active laundry available in the system");
                orderDto = OrderDto.FromOrder(order);
            }

            return orderDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(string orderId, OrderStatus newStatus, string? comment = null)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        var oldStatus = order.Status;
        var oldCourierId = order.CourierId;

        switch (newStatus)
        {
            case OrderStatus.Created:
                throw new InvalidOperationException("Cannot set status to Created");
            case OrderStatus.AcceptedByLaundry:
                order.LaundryAccepted();
                break;
            case OrderStatus.LaundryRejected:
                order.LaundryRejected(comment ?? "No reason provided");
                break;
            case OrderStatus.AwaitingPickup:
                order.AwaitCourierPickup();
                await _orderRepository.UpdateAsync(order);
                await _courierNotificationService.NotifyNewOrderAsync(OrderDto.FromOrder(order));
                break;
            case OrderStatus.PickupRejected:
                order.PickupRejected(comment ?? "No reason provided");
                break;
            case OrderStatus.PickedUp:
                order.PickedUp();
                break;
            case OrderStatus.InLaundry:
                // When order arrives at laundry, unassign courier but keep the history
                if (oldCourierId != null)
                {
                    await _courierService.AddCompletedOrderAsync(oldCourierId, orderId, "Delivered to laundry");
                    order.UnassignCourier();
                }
                order.InLaundry();
                break;
            case OrderStatus.ReadyForDelivery:
                order.ReadyForDelivery();
                await _orderRepository.UpdateAsync(order);
                await _courierNotificationService.NotifyNewOrderAsync(OrderDto.FromOrder(order));
                break;
            case OrderStatus.OutForDelivery:
                order.OutForDelivery();
                break;
            case OrderStatus.DeliveryRejected:
                order.DeliveryRejected(comment ?? "No reason provided");
                break;
            case OrderStatus.Delivered:
                // When order is delivered to customer, unassign courier but keep the history
                if (oldCourierId != null)
                {
                    await _courierService.AddCompletedOrderAsync(oldCourierId, orderId, "Delivered to customer");
                    order.UnassignCourier();
                }
                order.Delivered();
                break;
            case OrderStatus.Cancelled:
                order.Cancel(comment ?? "No reason provided");
                break;
            default:
                throw new InvalidOperationException($"Status change to {newStatus} is not supported");
        }

        await _orderRepository.UpdateAsync(order);
        return OrderDto.FromOrder(order);
    }

    public async Task<bool> AssignLaundryAsync(string orderId, string laundryId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return false;
        }

        var laundry = await _userRepository.GetByIdAsync<User>(laundryId);
        if (laundry == null || !await _userManager.IsInRoleAsync(laundry, "LaundryManager"))
        {
            return false;
        }

        order.AssignLaundry(laundryId);
        await _orderRepository.UpdateAsync(order);
        return true;
    }

    public async Task<OrderDto> AssignCourierAsync(string orderId, string courierId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var courier = await _userRepository.GetByIdAsync<User>(courierId);
        if (courier == null || !await _userManager.IsInRoleAsync(courier, "Courier"))
        {
            throw new InvalidOperationException($"Invalid courier {courierId}");
        }

        order.AssignCourier(courierId);
        await _orderRepository.UpdateAsync(order);
        return OrderDto.FromOrder(order);
    }

    public async Task<OrderDto> AcceptOrderAsync(string orderId, string laundryId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        if (order.LaundryId != null && order.LaundryId != laundryId)
            throw new InvalidOperationException("Order is already assigned to a different laundry");

        order.AssignLaundry(laundryId);
        order.LaundryAccepted();
        await _orderRepository.UpdateAsync(order);
        
        // Now update to awaiting pickup
        await UpdateOrderStatusAsync(orderId, OrderStatus.AwaitingPickup, "Order ready for courier pickup");
        order = await _orderRepository.GetByIdAsync(orderId);
        var orderDto = OrderDto.FromOrder(order);

        // Notify all couriers about the new order
        await _courierNotificationService.NotifyNewOrderAsync(orderDto);

        return orderDto;
    }

    public async Task<OrderDto> DeclineOrderAsync(string orderId, string laundryId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        if (order.LaundryId != null && order.LaundryId != laundryId)
            throw new InvalidOperationException("Order is already assigned to a different laundry");

        order.LaundryRejected("Order declined by laundry");

        await _orderRepository.UpdateAsync(order);
        return OrderDto.FromOrder(order);
    }

    public async Task<List<OrderDto>> GetPendingOrdersAsync()
    {
        var orders = await _orderRepository.GetPendingOrdersAsync();
        return orders.Select(OrderDto.FromOrder).ToList();
    }

    public async Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
    {
        var orders = await _orderRepository.GetOrdersByStatusAsync(status);
        return orders.Select(OrderDto.FromOrder).ToList();
    }

    public async Task<List<OrderDto>> GetOrdersByStatusesAsync(IEnumerable<OrderStatus> statuses)
    {
        var orders = await _orderRepository.GetOrdersByStatusesAsync(statuses);
        return orders.Select(OrderDto.FromOrder).ToList();
    }

    public async Task<OrderDto> StartProcessingAsync(string orderId, string laundryId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        if (order.LaundryId != laundryId)
            throw new InvalidOperationException("Order belongs to a different laundry");

        if (order.Status != OrderStatus.AcceptedByLaundry)
            throw new InvalidOperationException($"Cannot start processing order in {order.Status} status");

        order.StartProcessing();
        await _orderRepository.UpdateAsync(order);
        return OrderDto.FromOrder(order);
    }

    public async Task<OrderDto> MarkAsReadyAsync(string orderId, string laundryId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        if (order.LaundryId != laundryId)
            throw new InvalidOperationException("Order belongs to a different laundry");

        if (order.Status != OrderStatus.InLaundry)
            throw new InvalidOperationException($"Cannot mark as ready order in {order.Status} status");

        order.MarkAsReady();
        await _orderRepository.UpdateAsync(order);
        return OrderDto.FromOrder(order);
    }
} 