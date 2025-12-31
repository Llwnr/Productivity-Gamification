using Gamification.Core.GameModels;
using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Microsoft.EntityFrameworkCore;
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
        Task scheduledTask = ExecuteScheduledAsync(stoppingToken);
        
        await Task.WhenAll(periodicTask, scheduledTask);
        
        _logger.LogInformation("Scheduling service has stopped");
    }

    private async Task ExecutePeriodicAsync(CancellationToken stoppingToken){
        while (!stoppingToken.IsCancellationRequested){
            await Task.Delay(1000*60*5, stoppingToken);
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
                _logger.LogInformation("Pruning old and obsolete productivity data");
                ProductivityDbContext dbContext = scope.ServiceProvider.GetRequiredService<ProductivityDbContext>();
                await PruneObsoleteData(dbContext);
                _logger.LogInformation("Managing daily streaks...");
                IStreakManagementService  activityProcessingService = scope.ServiceProvider.GetRequiredService<IStreakManagementService>();
                await activityProcessingService.ManageDailyStreak();
                
                //Perform weekly operations
                if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday){
                    _logger.LogInformation("Managing weekly streaks...");
                    await activityProcessingService.ManageWeeklyStreak();
                }
            }
        }
    }
    
    async Task PruneObsoleteData(ProductivityDbContext dbContext){
        User[] users = await dbContext.Users
            .Include(u => u.ProductivityLogs)
            .Include(u => u.GameStat)
            .ToArrayAsync();
        foreach (var user in users){
            ProductivityLog[]? logs = user.ProductivityLogs?.ToArray();
            if (logs == null || logs.Length == 0) return;

            DateTime today = DateTime.UtcNow.Date;
            var timePeriods = new Dictionary<GameStat.TimeFrequency, Func<DateTime, bool>>{
                {GameStat.TimeFrequency.Daily, date => date.Date >= today},
                {GameStat.TimeFrequency.Weekly, date => date.Date >= today.AddDays(-6)},
                {GameStat.TimeFrequency.Monthly, date => date.Date >= today.AddDays(-30)},
                {GameStat.TimeFrequency.Yearly, date => date.Date >= today.AddDays(-365)},
                {GameStat.TimeFrequency.Lifetime, date => true},
            };

            foreach (var timePeriod in timePeriods){
                TimeSpan productiveTime = TimeSpan.FromTicks(logs
                    .Where(log => timePeriod.Value(log.LogDate))
                    .Sum(log => log.ProductiveTime.Ticks));

                user.GameStat.ProductivityMetrics[timePeriod.Key] = productiveTime;
                Console.WriteLine("Productive time on day:" + timePeriod.Key + " : " + productiveTime);
            }
            //To notify change in a JSON object, so efcore knows it MUST update this JSON property. Otherwise it just ignores it.
            dbContext.Entry(user.GameStat).Property(u => u.ProductivityMetrics).IsModified = true;
            await dbContext.SaveChangesAsync();
        }
    }
}