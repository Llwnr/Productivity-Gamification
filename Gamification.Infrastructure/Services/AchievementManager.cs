using System.Threading.Channels;
using Gamification.Infrastructure.ChannelData;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;

public class AchievementManager : BackgroundService{
    private readonly Channel<AchievementMessage> _channel;
    private ILogger<AchievementManager> _logger;
    public AchievementManager(Channel<AchievementMessage> channel, ILogger<AchievementManager> logger){
        _channel = channel;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        while (await _channel.Reader.WaitToReadAsync(stoppingToken)){
            var request = await _channel.Reader.ReadAsync(stoppingToken);
            _logger.LogInformation("Received message: "  + request.Message);
        }
    }
}