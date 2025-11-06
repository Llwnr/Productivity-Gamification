using System.Threading.Channels;
using Gamification.Core.GameModels;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;

public class AchievementManager : BackgroundService{
    private readonly Channel<GameEvent> _channel;
    private ILogger<AchievementManager> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    
    public AchievementManager(Channel<GameEvent> channel, ILogger<AchievementManager> logger, IServiceScopeFactory scopeFactory){
        _channel = channel;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        while (await _channel.Reader.WaitToReadAsync(stoppingToken)){
            var request = await _channel.Reader.ReadAsync(stoppingToken);
            using (var scope = _scopeFactory.CreateScope()){
                ProductivityDbContext dbContext = scope.ServiceProvider.GetRequiredService<ProductivityDbContext>();
                EvaluateAchievements(request, dbContext);
            }
        }
        
        void EvaluateAchievements(GameEvent request, ProductivityDbContext dbContext){
            foreach (var achievementRule in AchievementRules.Rules){
                achievementRule.Evaluate(request, AddToUserAchievements);
            }
            
            void AddToUserAchievements(User user, string achievementKey){
                Achievement? achievement = dbContext.Achievements.FirstOrDefault(a => a.Key == achievementKey);
                    
                bool achievementAlreadyOwned = dbContext.UserAchievements.Any(ua => ua.UserId == user.UserId && ua.Achievement.Key == achievementKey);
                if (achievementAlreadyOwned){
                    _logger.LogInformation("The user already owns this achievement.");
                    return;
                }
                    
                if (achievement == null){
                    throw new Exception("No achievement found with the key: " + achievementKey);
                }
                dbContext.UserAchievements.Add(new UserAchievement{
                    UserId = user.UserId,
                    Achievement = dbContext.Achievements.First(a => a.Key == achievementKey),
                    EarnedAt = DateTime.UtcNow
                });

                dbContext.SaveChanges();
                _logger.LogInformation($"Successfully added the achievement {achievementKey} to user {user.Username}");
            }
        }
    }
}