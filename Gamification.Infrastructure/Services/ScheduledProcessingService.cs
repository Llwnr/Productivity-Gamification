using Gamification.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;

public class ScheduledProcessingService : BackgroundService{
    private readonly IServiceScopeFactory  _scopeFactory;
    private readonly ILogger<ScheduledProcessingService> _logger;

    public ScheduledProcessingService(ILogger<ScheduledProcessingService> logger, IServiceScopeFactory scopeFactory){
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        while (!stoppingToken.IsCancellationRequested){
            await Task.Delay(1000*10, stoppingToken);
            using (var scope = _scopeFactory.CreateScope()){
                IActivityProcessingService  activityProcessingService = scope.ServiceProvider.GetRequiredService<IActivityProcessingService>();
                _logger.LogInformation("Processing score...");
                await activityProcessingService.ProcessUserActivityAsync();
            }
        }
    }
}