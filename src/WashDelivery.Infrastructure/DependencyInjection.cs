using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.Interfaces;
using WashDelivery.Infrastructure.Data;
using WashDelivery.Infrastructure.Services;
using WashDelivery.Application.Services;
using WashDelivery.Infrastructure.Data.Repositories;
using WashDelivery.Domain.Entities;
using WashDelivery.Infrastructure.Repositories;

namespace WashDelivery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILaundryService, WashDelivery.Infrastructure.Services.LaundryService>();
        services.AddHttpClient<IGeocodingService, GeocodingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDraftOrderService, DraftOrderService>();
        services.AddScoped<ICourierService, CourierService>();
        services.AddScoped<ICourierNotificationService, CourierNotificationService>();
        services.AddScoped<IOrderAssignmentService, OrderAssignmentService>();
        services.AddScoped<ILaundryNotificationService, LaundryNotificationService>();

        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register repositories
        services.AddScoped<ILaundryRepository, LaundryRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
        services.AddScoped<IDraftOrderRepository, DraftOrderRepository>();

        // Add logging
        services.AddLogging();

        return services;
    }
} 