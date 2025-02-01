using System;

namespace WashDelivery.Domain.Entities;

public class LaundryOrderView
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; }
    public string LaundryId { get; set; }
    public DateTime FirstSeenAt { get; set; }
    public bool HasResponded { get; set; }
    
    public virtual Order Order { get; set; }
    public virtual Laundry Laundry { get; set; }
} 