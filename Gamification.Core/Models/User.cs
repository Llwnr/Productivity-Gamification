using Gamification.Core.GameModels;

namespace Gamification.Core.Models;

public class User{
    public string UserId{ get; set; }
    
    public string? Username{ get; set; }
    public string? Email{ get; set; }
    public string? Password{ get; set; } //Hashed password btw, not text
    
    public string? Goal{ get; set; }
    public TimeSpan DailyTargetHours{ get; set; }
    
    public GameStat GameStat{ get; set; } //Nav property
    public ICollection<UserSiteVisit>? UserSiteVisits{ get; set; }
    public ICollection<UserAchievement>?  UserAchievements{ get; set; }
    public ICollection<ProductivityLog>? ProductivityLogs{ get; set; }
}