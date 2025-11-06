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
        _logger.LogInformation("Starting up scheduling service");
        
        Task periodicTask = ExecutePeriodicAsync(stoppingToken);
        // Task scheduledTask = ExecuteScheduledAsync(stoppingToken);
        
        await Task.WhenAll(periodicTask);
        
        _logger.LogInformation("Scheduling service has stopped");
    }

    private async Task ExecutePeriodicAsync(CancellationToken stoppingToken){
        while (!stoppingToken.IsCancellationRequested){
            await Task.Delay(1000*5, stoppingToken);
            using (var scope = _scopeFactory.CreateScope()){
                IActivityProcessingService  activityProcessingService = scope.ServiceProvider.GetRequiredService<IActivityProcessingService>();
                _logger.LogInformation("Processing score...");
                await activityProcessingService.ProcessUserActivityAsync();
            }
        }
    }

    private async Task ExecuteScheduledAsync(CancellationToken stoppingToken){
        while (!stoppingToken.IsCancellationRequested){
            DateTime nextMidnight = DateTime.UtcNow.Date.AddDays(1);
            TimeSpan delay = nextMidnight - DateTime.UtcNow;

            if (delay < TimeSpan.Zero){
                delay = nextMidnight.AddDays(1) - DateTime.UtcNow;
            }
            //Wait 1 day to run the code below
            await Task.Delay(delay, stoppingToken);
            
            using (var scope = _scopeFactory.CreateScope()){
                _logger.LogInformation("Managing daily streaks...");
                IStreakManagementService  activityProcessingService = scope.ServiceProvider.GetRequiredService<IStreakManagementService>();
                await activityProcessingService.ManageDailyStreak();
                
                //Perform weekly operations
                if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday){
                    _logger.LogInformation("Managing weekly streaks...");
                    await activityProcessingService.ManageWeeklyStreak();
                }

                
                
                //Manages the user's total productivity time spent daily, weekly, monthly, yearly etc by removing older data from them.
                void PruneOldProductivitiyMetrics(){
                    
                }
            }
        }
    }
}