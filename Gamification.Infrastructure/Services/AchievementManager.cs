using System.Threading.Channels;
using Gamification.Infrastructure.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;

public class AchievementManager : BackgroundService{
    private readonly Channel<GameEvent> _channel;
    private ILogger<AchievementManager> _logger;
    
    public AchievementManager(Channel<GameEvent> channel, ILogger<AchievementManager> logger){
        _channel = channel;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        while (await _channel.Reader.WaitToReadAsync(stoppingToken)){
            var request = await _channel.Reader.ReadAsync(stoppingToken);
            if (request is ExpGainedEvent req){
                _logger.LogInformation($"Exp Event triggered. User {req.UserId} gained {req.GainedExp}. Total exp is now {req.TotalExp}");
            }
        }
    }
}