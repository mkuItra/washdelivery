using Microsoft.Extensions.DependencyInjection;
using WashDelivery.Application.Interfaces;
using WashDelivery.Infrastructure.Services;

namespace WashDelivery.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        // Add any web-specific services here
        return services;
    }
} 