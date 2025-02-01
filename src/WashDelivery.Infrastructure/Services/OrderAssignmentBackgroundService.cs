using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WashDelivery.Application.Interfaces;

namespace WashDelivery.Infrastructure.Services;

public class OrderAssignmentBackgroundService : BackgroundService
{
    private readonly ILogger<OrderAssignmentBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderAssignmentBackgroundService(
        ILogger<OrderAssignmentBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[OrderAssignmentBackground] Background service starting");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("[OrderAssignmentBackground] Starting processing iteration");
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var orderAssignmentService = scope.ServiceProvider.GetRequiredService<IOrderAssignmentService>();
                    _logger.LogInformation("[OrderAssignmentBackground] Got OrderAssignmentService from scope, processing pending orders");
                    await orderAssignmentService.ProcessPendingOrdersAsync(stoppingToken);
                    _logger.LogInformation("[OrderAssignmentBackground] Successfully processed pending orders");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OrderAssignmentBackground] Error occurred while processing pending orders");
            }

            _logger.LogInformation("[OrderAssignmentBackground] Waiting for next iteration");
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        
        _logger.LogInformation("[OrderAssignmentBackground] Background service stopping");
    }
} 