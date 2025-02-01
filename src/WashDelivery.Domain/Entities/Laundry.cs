using System;
using System.Collections.Generic;
using WashDelivery.Domain.ValueObjects;

namespace WashDelivery.Domain.Entities;

public class Laundry : IBaseEntity
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string ContactEmail { get; private set; }
    public string ContactPhone { get; private set; }
    public bool IsActive { get; private set; }
    public double Rating { get; private set; }
    public LocationAddress Address { get; private set; }
    private readonly List<LaundryService> _services = new();
    public IReadOnlyCollection<LaundryService> Services => _services.AsReadOnly();
    private readonly List<LaundryWorker> _workers = new();
    public IReadOnlyCollection<LaundryWorker> Workers => _workers.AsReadOnly();

    protected Laundry() { }

    public Laundry(
        string name,
        string contactEmail,
        string contactPhone,
        LocationAddress address)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Address = address;
        IsActive = true;
        Rating = 0;
    }

    public void AddService(LaundryService service)
    {
        if (!_services.Any(s => s.Id == service.Id))
        {
            _services.Add(service);
        }
    }

    public void AddWorker(LaundryWorker worker)
    {
        if (!_workers.Any(w => w.Id == worker.Id))
        {
            _workers.Add(worker);
        }
    }

    public void RemoveWorker(LaundryWorker worker)
    {
        _workers.Remove(worker);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void Activate() => SetActive(true);
    public void Deactivate() => SetActive(false);

    public void UpdateRating(double rating)
    {
        Rating = rating;
    }

    public void Update(
        string name,
        string contactEmail,
        string contactPhone,
        LocationAddress address)
    {
        Name = name;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Address = address;
    }

    public void UpdateLocation(
        string street,
        string city,
        string postalCode,
        double latitude,
        double longitude)
    {
        Address = new LocationAddress(street, city, postalCode, latitude, longitude);
    }
} 