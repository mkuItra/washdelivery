using AutoMapper;
using WashDelivery.Application.DTOs.Orders;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.ValueObjects;

namespace WashDelivery.Application.Mapping;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.LaundryId, opt => opt.MapFrom(src => src.LaundryId))
            .ForMember(dest => dest.CourierId, opt => opt.MapFrom(src => src.CourierId))
            .ForMember(dest => dest.PickupTime, opt => opt.MapFrom(src => src.PickupTime))
            .ForMember(dest => dest.LeaveAtDoor, opt => opt.MapFrom(src => src.LeaveAtDoor))
            .ForMember(dest => dest.CourierInstructions, opt => opt.MapFrom(src => src.CourierInstructions))
            .ForMember(dest => dest.PickupAddress, opt => opt.MapFrom(src => src.PickupAddress))
            .ForMember(dest => dest.DeliveryAddress, opt => opt.MapFrom(src => src.DeliveryAddress))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.StatusHistory, opt => opt.MapFrom(src => src.StatusHistory))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src => src.FinalPrice));

        CreateMap<OrderItem, OrderItemDetailsDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight))
            .ForMember(dest => dest.IsExtra, opt => opt.MapFrom(src => src.IsExtra))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => 1));

        CreateMap<OrderAddress, OrderAddressDto>();
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Note))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.ChangedAt));
    }
} 