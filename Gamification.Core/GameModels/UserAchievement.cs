using Gamification.Core.Models;

namespace Gamification.Core.GameModels;

public class UserAchievement{
    public string UserAchievementId{ get; set; }
    
    public string UserId{ get; set; }
    public User User{ get; set; }
    
    public string AchievementId{ get; set; }
    public Achievement Achievement{ get; set; }
    
    public DateTime EarnedAt{ get; set; }
}